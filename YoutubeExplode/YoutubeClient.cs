using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YoutubeExplode.Internal;
using YoutubeExplode.Models;
using YoutubeExplode.Services;

namespace YoutubeExplode
{
    /// <summary>
    /// YoutubeClient
    /// </summary>
    public partial class YoutubeClient
    {
        private readonly IHttpService _httpService;
        private readonly Dictionary<string, PlayerSource> _playerSourceCache;

        /// <summary>
        /// Creates an instance of <see cref="YoutubeClient"/> with custom services
        /// </summary>
        public YoutubeClient(IHttpService httpService)
        {
            _httpService = httpService ?? throw new ArgumentNullException(nameof(httpService));
            _playerSourceCache = new Dictionary<string, PlayerSource>();
        }

        /// <summary>
        /// Creates an instance of <see cref="YoutubeClient"/> with default services
        /// </summary>
        public YoutubeClient()
            : this(HttpService.Instance)
        {
        }

        private async Task<PlayerSource> GetPlayerSourceAsync(string version)
        {
            // Try get cached player source
            var playerSource = _playerSourceCache.GetOrDefault(version);

            // If not available - decompile a new one
            if (playerSource == null)
            {
                // Get
                string url = $"https://www.youtube.com/yts/jsbin/player-{version}/base.js";
                string response = await _httpService.GetStringAsync(url).ConfigureAwait(false);

                // Parse
                playerSource = Parser.PlayerSourceFromJs(response);
                playerSource.Version = version;

                // Cache
                _playerSourceCache[version] = playerSource;
            }

            return playerSource;
        }

        private async Task<long> GetContentLengthAsync(string url)
        {
            // Get the headers
            var headers = await _httpService.GetHeadersAsync(url).ConfigureAwait(false);

            // Get file size header
            string cl = headers.GetOrDefault("Content-Length");
            if (cl == null)
                throw new KeyNotFoundException("Content-Length header not found");

            return cl.ParseLongOrDefault();
        }

        /// <summary>
        /// Checks whether a video with the given ID exists
        /// </summary>
        public async Task<bool> CheckVideoExistsAsync(string videoId)
        {
            if (videoId == null)
                throw new ArgumentNullException(nameof(videoId));
            if (!ValidateVideoId(videoId))
                throw new ArgumentException("Invalid Youtube video ID", nameof(videoId));

            // Get
            string url = $"https://www.youtube.com/get_video_info?video_id={videoId}&el=info&ps=default";
            string response = await _httpService.GetStringAsync(url).ConfigureAwait(false);

            // Parse
            var dic = Parser.DictionaryFromUrlEncoded(response);

            // Check status
            string status = dic.GetOrDefault("status");
            if (status.EqualsInvariant("fail"))
            {
                // If there's an error, only 100 and 150 indicate non-existing video
                int errorCode = dic.GetOrDefault("errorcode").ParseIntOrDefault();
                return !errorCode.IsEither(100, 150);
            }

            return true;
        }

        /// <summary>
        /// Gets video metadata by video ID
        /// </summary>
        public async Task<VideoInfo> GetVideoInfoAsync(string videoId)
        {
            if (videoId == null)
                throw new ArgumentNullException(nameof(videoId));
            if (!ValidateVideoId(videoId))
                throw new ArgumentException("Invalid Youtube video ID", nameof(videoId));

            // Get video context
            string url = $"https://www.youtube.com/embed/{videoId}";
            string response = await _httpService.GetStringAsync(url).ConfigureAwait(false);

            // Parse video context
            var videoContext = Parser.VideoContextFromHtml(response);

            // Get video info
            url =
                $"https://www.youtube.com/get_video_info?video_id={videoId}&sts={videoContext.Sts}&el=info&ps=default";
            response = await _httpService.GetStringAsync(url).ConfigureAwait(false);

            // Parse video info
            var result = Parser.VideoInfoFromUrlEncoded(response);

            // Get video info extension
            url = $"https://www.youtube.com/get_video_metadata?video_id={videoId}";
            response = await _httpService.GetStringAsync(url).ConfigureAwait(false);

            // Parse video info extension and copy metadata
            var resultExtension = Parser.VideoInfoFromXml(response);
            result.Author = resultExtension.Author;
            result.Description = resultExtension.Description;
            result.LikeCount = resultExtension.LikeCount;
            result.DislikeCount = resultExtension.DislikeCount;

            // Check if dash manifest is available
            if (result.DashManifest != null)
            {
                // Decipher if needed
                if (result.DashManifest.NeedsDeciphering)
                {
                    // Get player source
                    var playerSource = await GetPlayerSourceAsync(videoContext.PlayerVersion).ConfigureAwait(false);

                    // Update signature
                    string sig = result.DashManifest.Signature;
                    string newSig = playerSource.Decipher(sig);
                    result.DashManifest.Url = result.DashManifest.Url.SetPathParameter("signature", newSig);
                    result.DashManifest.NeedsDeciphering = false;
                }

                // Get dash manifest
                response = await _httpService.GetStringAsync(result.DashManifest.Url).ConfigureAwait(false);

                // Parse and add new streams
                var dashStreams = Parser.MediaStreamInfosFromXml(response);
                result.Streams = result.Streams.Concat(dashStreams).ToArray();
            }

            // Decipher streams if needed
            if (result.Streams.Any(s => s.NeedsDeciphering))
            {
                // Get player source
                var playerSource = await GetPlayerSourceAsync(videoContext.PlayerVersion).ConfigureAwait(false);

                // Update signatures
                foreach (var streamInfo in result.Streams.Where(s => s.NeedsDeciphering))
                {
                    string sig = streamInfo.Signature;
                    string newSig = playerSource.Decipher(sig);
                    streamInfo.Url = streamInfo.Url.SetQueryParameter("signature", newSig);
                    streamInfo.NeedsDeciphering = false;
                }
            }

            // Get file size of streams that don't have it yet
            foreach (var streamInfo in result.Streams.Where(s => s.FileSize == 0))
            {
                streamInfo.FileSize = await GetContentLengthAsync(streamInfo.Url).ConfigureAwait(false);
            }

            // Finalize the stream list
            result.Streams = result.Streams
                .Distinct(s => s.Itag) // only one stream per itag
                .OrderByDescending(s => s.Quality) // sort by quality
                .ThenByDescending(s => s.Bitrate) // then by bitrate
                .ThenByDescending(s => s.FileSize) // then by filesize
                .ThenByDescending(s => s.ContainerType) // then by type
                .ToArray();

            return result;
        }

        /// <summary>
        /// Gets playlist metadata by playlist ID, truncating video list at given number of pages
        /// </summary>
        public async Task<PlaylistInfo> GetPlaylistInfoAsync(string playlistId, int maxPages)
        {
            // Original code credit: https://github.com/dr-BEat

            if (playlistId == null)
                throw new ArgumentNullException(nameof(playlistId));
            if (!ValidatePlaylistId(playlistId))
                throw new ArgumentException("Invalid Youtube playlist ID", nameof(playlistId));
            if (maxPages <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxPages), "Needs to be a positive number");

            // Get
            string url = $"https://www.youtube.com/list_ajax?style=xml&action_get_list=1&list={playlistId}";
            string response = await _httpService.GetStringAsync(url).ConfigureAwait(false);

            // Parse
            var result = Parser.PlaylistInfoFromXml(response);
            result.Id = playlistId;

            // Continue with next pages
            int pagesDone = 1;
            bool canContinue = result.VideoIds.Count > 0;
            int offset = result.VideoIds.Count;
            while (pagesDone < maxPages && canContinue)
            {
                // Get
                string nextUrl = url + $"&index={offset}";
                response = await _httpService.GetStringAsync(nextUrl).ConfigureAwait(false);

                // Parse and concat IDs
                var resultExtension = Parser.PlaylistInfoFromXml(response);
                int delta = result.VideoIds.Count;
                result.VideoIds = result.VideoIds.Union(resultExtension.VideoIds).ToArray();
                delta = result.VideoIds.Count - delta;

                // Go for the next batch if needed
                pagesDone++;
                canContinue = delta > 0;
                offset += resultExtension.VideoIds.Count;
            }

            return result;
        }

        /// <summary>
        /// Gets playlist metadata by playlist ID
        /// </summary>
        public async Task<PlaylistInfo> GetPlaylistInfoAsync(string playlistId)
            => await GetPlaylistInfoAsync(playlistId, int.MaxValue).ConfigureAwait(false);

        /// <summary>
        /// Gets actual media stream by its metadata
        /// </summary>
        public async Task<MediaStream> GetMediaStreamAsync(MediaStreamInfo mediaStreamInfo)
        {
            if (mediaStreamInfo == null)
                throw new ArgumentNullException(nameof(mediaStreamInfo));

            // Get
            var stream = await _httpService.GetStreamAsync(mediaStreamInfo.Url).ConfigureAwait(false);

            // Pack
            var result = new MediaStream(stream, mediaStreamInfo);

            return result;
        }

        /// <summary>
        /// Gets actual closed caption track by its metadata
        /// </summary>
        public async Task<ClosedCaptionTrack> GetClosedCaptionTrackAsync(ClosedCaptionTrackInfo closedCaptionTrackInfo)
        {
            if (closedCaptionTrackInfo == null)
                throw new ArgumentNullException(nameof(closedCaptionTrackInfo));

            // Get
            string response = await _httpService.GetStringAsync(closedCaptionTrackInfo.Url).ConfigureAwait(false);

            // Parse
            var result = Parser.ClosedCaptionTrackFromXml(response);
            result.Info = closedCaptionTrackInfo;

            return result;
        }
    }
}