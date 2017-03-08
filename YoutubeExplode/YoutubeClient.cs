using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YoutubeExplode.Internal;
using YoutubeExplode.Models;
using YoutubeExplode.Services;

namespace YoutubeExplode
{
    /// <summary>
    /// YoutubeClient
    /// </summary>
    public partial class YoutubeClient : IDisposable
    {
        private readonly IRequestService _requestService;
        private readonly Dictionary<string, PlayerSource> _playerSourceCache = new Dictionary<string, PlayerSource>();

        /// <summary>
        /// Creates an instance of <see cref="YoutubeClient"/> with custom services
        /// </summary>
        public YoutubeClient(IRequestService requestService)
        {
            _requestService = requestService;
        }

        /// <summary>
        /// Creates an instance of <see cref="YoutubeClient"/> with default services
        /// </summary>
        public YoutubeClient()
            : this(new DefaultRequestService())
        {
        }

        private async Task<PlayerSource> GetPlayerSourceAsync(string version)
        {
            if (version.IsBlank())
                throw new ArgumentNullException(nameof(version));

            // Try get cached player source
            var playerSource = _playerSourceCache.GetOrDefault(version);

            // If not available - decompile a new one
            if (playerSource == null)
            {
                // Get the javascript source
                string url = $"https://www.youtube.com/yts/jsbin/player-{version}/base.js";
                string response = await _requestService.GetStringAsync(url).ConfigureAwait(false);
                if (response.IsBlank())
                    throw new Exception("Could not get the video player source code");

                // Decompile
                playerSource = Parser.PlayerSourceFromJs(response);
                playerSource.Version = version;

                // Cache
                _playerSourceCache[version] = playerSource;
            }

            return playerSource;
        }

        private async Task DecipherAsync(VideoInfo videoInfo, string playerVersion)
        {
            if (videoInfo == null)
                throw new ArgumentNullException(nameof(videoInfo));
            if (!videoInfo.NeedsDeciphering)
                throw new Exception("Given video info does not need to be deciphered");

            // Get player source
            var playerSource = await GetPlayerSourceAsync(playerVersion).ConfigureAwait(false);

            // Decipher streams
            foreach (var streamInfo in videoInfo.Streams.Where(s => s.NeedsDeciphering))
            {
                string sig = streamInfo.Signature;
                string newSig = playerSource.Decipher(sig);
                streamInfo.Url = streamInfo.Url.SetQueryParameter("signature", newSig);
                streamInfo.NeedsDeciphering = false;
            }

            // Decipher dash manifest
            if (videoInfo.DashManifest != null && videoInfo.DashManifest.NeedsDeciphering)
            {
                string sig = videoInfo.DashManifest.Signature;
                string newSig = playerSource.Decipher(sig);
                videoInfo.DashManifest.Url = videoInfo.DashManifest.Url.SetPathParameter("signature", newSig);
                videoInfo.DashManifest.NeedsDeciphering = false;
            }
        }

        private async Task<long> GetFileSizeAsync(VideoStreamInfo streamInfo)
        {
            if (streamInfo == null)
                throw new ArgumentNullException(nameof(streamInfo));
            if (streamInfo.Url.IsBlank())
                throw new Exception("Given stream does not have a URL");
            if (streamInfo.NeedsDeciphering)
                throw new Exception("Given stream's signature needs to be deciphered first");

            // Get the headers
            var headers = await _requestService.GetHeadersAsync(streamInfo.Url).ConfigureAwait(false);
            if (headers == null)
                throw new Exception("Could not get headers");

            // Get file size header
            if (!headers.ContainsKey("Content-Length"))
                throw new Exception("Content-Length header not found");

            return streamInfo.FileSize = headers["Content-Length"].ParseLong();
        }

        /// <summary>
        /// Checks whether a video with the given ID exists
        /// </summary>
        public async Task<bool> CheckVideoExistsAsync(string videoId)
        {
            if (videoId.IsBlank())
                throw new ArgumentNullException(nameof(videoId));
            if (!ValidateVideoId(videoId))
                throw new ArgumentException("Is not a valid Youtube video ID", nameof(videoId));

            // Get the video info
            string url = $"https://www.youtube.com/get_video_info?video_id={videoId}";
            string response = await _requestService.GetStringAsync(url).ConfigureAwait(false);
            if (response.IsBlank())
                throw new Exception("Could not get video info");

            // Parse
            var dic = Parser.DictionaryFromUrlEncoded(response);
            string status = dic.GetOrDefault("status");
            int errorCode = dic.GetOrDefault("errorcode").ParseIntOrDefault();
            return !(status.EqualsInvariant("fail") && errorCode.IsEither(100, 150));
        }

        /// <summary>
        /// Gets video meta data by ID
        /// </summary>
        public async Task<VideoInfo> GetVideoInfoAsync(string videoId)
        {
            if (videoId.IsBlank())
                throw new ArgumentNullException(nameof(videoId));
            if (!ValidateVideoId(videoId))
                throw new ArgumentException("Is not a valid Youtube video ID", nameof(videoId));

            // Get video context
            string url = $"https://www.youtube.com/embed/{videoId}";
            string response = await _requestService.GetStringAsync(url).ConfigureAwait(false);
            if (response.IsBlank())
                throw new Exception("Could not get video context");

            // Parse video context
            var videoContext = Parser.VideoContextFromHtml(response);

            // Get video info
            url = $"https://www.youtube.com/get_video_info?video_id={videoId}&sts={videoContext.Sts}";
            response = await _requestService.GetStringAsync(url).ConfigureAwait(false);
            if (response.IsBlank())
                throw new Exception("Could not get video info");

            // Parse video info
            var result = Parser.VideoInfoFromUrlEncoded(response);

            // Decipher
            if (result.NeedsDeciphering)
            {
                await DecipherAsync(result, videoContext.PlayerVersion).ConfigureAwait(false);
            }

            // Get additional streams from dash if available
            if (result.DashManifest != null)
            {
                // Get
                response = await _requestService.GetStringAsync(result.DashManifest.Url).ConfigureAwait(false);
                if (response.IsBlank())
                    throw new Exception("Could not get dash manifest");

                // Parse
                var dashStreams = Parser.VideoStreamInfosFromMpd(response);
                result.Streams = result.Streams.With(dashStreams).ToArray();
            }

            // Finalize the stream list
            result.Streams = result.Streams
                .Distinct(s => s.Itag) // only one stream per itag
                .OrderByDescending(s => s.Quality) // sort by quality
                .ThenByDescending(s => s.Bitrate) // then by bitrate
                .ThenByDescending(s => s.FileSize) // then by filesize
                .ThenByDescending(s => s.Type) // then by type
                .ToArray();

            // Get file size of streams
            foreach (var streamInfo in result.Streams.Where(s => s.FileSize == 0))
                await GetFileSizeAsync(streamInfo).ConfigureAwait(false);

            return result;
        }

        /// <summary>
        /// Gets playlist meta data by id
        /// </summary>
        public async Task<PlaylistInfo> GetPlaylistInfoAsync(string playlistId)
        {
            if (playlistId.IsBlank())
                throw new ArgumentNullException(nameof(playlistId));
            if (!ValidateVideoId(playlistId))
                throw new ArgumentException("Is not a valid Youtube playlist ID", nameof(playlistId));

            // Set up urls
            string baseUrl = $"https://m.youtube.com/playlist?list={playlistId}&ajax=1&tsp=1&app=m";
            string url = baseUrl;

            // Set up content buffer to aggregate responses
            var buffer = new StringBuilder();

            // Loop to get all responses
            while (url.IsNotBlank())
            {
                // Get playlist info
                string response = await _requestService.GetStringAsync(url).ConfigureAwait(false);
                if (response.IsBlank())
                    throw new Exception("Could not get playlist info");

                // Add to buffer
                buffer.AppendLine(response);

                // Find continuation token
                string ctoken = Regex.Match(response, @"""continuation""\s*:\s*""(.*?)""").Groups[1].Value;
                if (ctoken.IsNotBlank())
                {
                    // If found - compose new url
                    url = baseUrl + "&action_continuation=1&ctoken=" + ctoken;
                }
                else
                {
                    // Otherwise - reset url
                    url = null;
                }
            }

            // Parse
            var result = Parser.PlaylistInfoFromJson(buffer.ToString());

            return result;
        }
        
        /// <summary>
        /// Gets the content of the given video stream
        /// </summary>
        public async Task<Stream> DownloadVideoAsync(VideoStreamInfo streamInfo)
        {
            if (streamInfo == null)
                throw new ArgumentNullException(nameof(streamInfo));
            if (streamInfo.Url.IsBlank())
                throw new Exception("Given stream does not have a URL");
            if (streamInfo.NeedsDeciphering)
                throw new Exception("Given stream's signature needs to be deciphered first");

            // Get stream
            var stream = await _requestService.GetStreamAsync(streamInfo.Url).ConfigureAwait(false);
            if (stream == null)
                throw new Exception("Could not get response stream");

            return stream;
        }

        /// <inheritdoc />
        public virtual void Dispose()
        {
            (_requestService as IDisposable)?.Dispose();
        }
    }

    public partial class YoutubeClient
    {
        /// <summary>
        /// Verifies that the given string is syntactically a valid youtube video ID
        /// </summary>
        public static bool ValidateVideoId(string videoId)
        {
            if (videoId.IsBlank())
                return false;

            // Seems to also have constant length of 11 (enforce?)

            return !Regex.IsMatch(videoId, @"[^0-9a-zA-Z_\-]");
        }

        /// <summary>
        /// Tries to parse video ID from a youtube video URL
        /// </summary>
        public static bool TryParseVideoId(string videoUrl, out string videoId)
        {
            videoId = default(string);

            if (videoUrl.IsBlank())
                return false;

            // https://www.youtube.com/watch?v=yIVRs6YSbOM
            string regularMatch = Regex.Match(videoUrl, @"youtube\..+?/watch\?.*?v=(.*?)(?:&|/|$)").Groups[1].Value;
            if (regularMatch.IsNotBlank() && ValidateVideoId(regularMatch))
            {
                videoId = regularMatch;
                return true;
            }

            // https://youtu.be/yIVRs6YSbOM
            string shortMatch = Regex.Match(videoUrl, @"youtu.be/(.*?)(?:&|/|$)").Groups[1].Value;
            if (shortMatch.IsNotBlank() && ValidateVideoId(shortMatch))
            {
                videoId = shortMatch;
                return true;
            }

            // https://www.youtube.com/embed/yIVRs6YSbOM
            string embedMatch = Regex.Match(videoUrl, @"youtube\..+?/embed/(.*?)(?:&|/|$)").Groups[1].Value;
            if (embedMatch.IsNotBlank() && ValidateVideoId(embedMatch))
            {
                videoId = embedMatch;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Parses video ID from a youtube video URL
        /// </summary>
        public static string ParseVideoId(string videoUrl)
        {
            if (videoUrl.IsBlank())
                throw new ArgumentNullException(nameof(videoUrl));

            string result;
            bool success = TryParseVideoId(videoUrl, out result);
            if (success)
                return result;

            throw new FormatException("Could not parse video ID from given string");
        }

        /// <summary>
        /// Verifies that the given string is syntactically a valid youtube playlist ID
        /// </summary>
        public static bool ValidatePlaylistId(string playlistId)
        {
            if (playlistId.IsBlank())
                return false;

            // Seems to also have constant length of 24 for system play lists (liked, favorites)
            // ... length of 34 for normal playlists
            // ... length of 2 for watch later playlist
            // (enforce?)

            return !Regex.IsMatch(playlistId, @"[^0-9a-zA-Z_\-]");
        }

        /// <summary>
        /// Tries to parse playlist ID from a youtube playlist URL
        /// </summary>
        public static bool TryParsePlaylistId(string playlistUrl, out string playlistId)
        {
            playlistId = default(string);

            if (playlistUrl.IsBlank())
                return false;

            // https://www.youtube.com/playlist?list=PLOU2XLYxmsIJGErt5rrCqaSGTMyyqNt2H
            var regularMatch = Regex.Match(playlistUrl, @"youtube\..+?/playlist\?.*?list=(.*?)(?:&|/|$)").Groups[1].Value;
            if (regularMatch.IsNotBlank() && ValidatePlaylistId(regularMatch))
            {
                playlistId = regularMatch;
                return true;
            }

            // https://www.youtube.com/watch?v=b8m9zhNAgKs&list=PL9tY0BWXOZFuFEG_GtOBZ8-8wbkH-NVAr
            var compositeMatch = Regex.Match(playlistUrl, @"youtube\..+?/watch\?.*?list=(.*?)(?:&|/|$)").Groups[1].Value;
            if (compositeMatch.IsNotBlank() && ValidatePlaylistId(compositeMatch))
            {
                playlistId = compositeMatch;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Parses playlist ID from a youtube playlist URL
        /// </summary>
        public static string ParsePlaylistId(string playlistUrl)
        {
            if (playlistUrl.IsBlank())
                throw new ArgumentNullException(nameof(playlistUrl));

            string result;
            bool success = TryParsePlaylistId(playlistUrl, out result);
            if (success)
                return result;

            throw new FormatException("Could not parse playlist ID from given string");
        }
    }
}