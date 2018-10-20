using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YoutubeExplode.Exceptions;
using YoutubeExplode.Internal;
using YoutubeExplode.Internal.Parsers;
using YoutubeExplode.Models;
using YoutubeExplode.Models.ClosedCaptions;
using YoutubeExplode.Models.MediaStreams;

namespace YoutubeExplode
{
    public partial class YoutubeClient
    {
        private async Task<VideoEmbedPageParser> GetVideoEmbedPageParserAsync(string videoId)
        {
            var url = $"https://www.youtube.com/embed/{videoId}?disable_polymer=true&hl=en";
            var raw = await _httpClient.GetStringAsync(url).ConfigureAwait(false);

            return VideoEmbedPageParser.Initialize(raw);
        }

        private async Task<VideoWatchPageParser> GetVideoWatchPageParserAsync(string videoId)
        {
            var url = $"https://www.youtube.com/watch?v={videoId}&disable_polymer=true&hl=en";
            var raw = await _httpClient.GetStringAsync(url).ConfigureAwait(false);

            return VideoWatchPageParser.Initialize(raw);
        }

        private async Task<VideoInfoParser> GetVideoInfoParserAsync(string videoId, string el, string sts)
        {
            // This parameter does magic and a lot of videos don't work without it
            var eurl = $"https://youtube.googleapis.com/v/{videoId}".UrlEncode();

            var url = $"https://www.youtube.com/get_video_info?video_id={videoId}&el={el}&sts={sts}&eurl={eurl}&hl=en";
            var raw = await _httpClient.GetStringAsync(url).ConfigureAwait(false);
            
            return VideoInfoParser.Initialize(raw);
        }

        private async Task<VideoInfoParser> GetVideoInfoParserAsync(string videoId, string sts = "")
        {
            // Get video info parser for 'el=embedded'
            var videoInfoParser = await GetVideoInfoParserAsync(videoId, "embedded", sts).ConfigureAwait(false);

            // Check if video exists by verifying that video ID property is not empty
            if (videoInfoParser.GetVideoId().IsBlank())
            {
                // Get native error code and error reason
                var errorCode = videoInfoParser.GetErrorCode();
                var errorReason = videoInfoParser.GetErrorReason();

                throw new VideoUnavailableException(videoId, errorCode, errorReason);
            }

            // If requested with "sts" parameter, it means that the calling code is interested in getting video info with streams.
            // For that we also need to make sure the video is fully available by checking for errors.
            if (sts.IsNotBlank() && videoInfoParser.GetErrorCode() != 0)
            {
                // Retry for "el=detailpage"
                videoInfoParser = await GetVideoInfoParserAsync(videoId, "detailpage", sts).ConfigureAwait(false);

                // If there are still errors - throw
                if (videoInfoParser.GetErrorCode() != 0)
                {
                    // Get native error code and error reason
                    var errorCode = videoInfoParser.GetErrorCode();
                    var errorReason = videoInfoParser.GetErrorReason();

                    throw new VideoUnavailableException(videoId, errorCode, errorReason);
                }
            }

            return videoInfoParser;
        }

        private async Task<PlayerContext> GetVideoPlayerContextAsync(string videoId)
        {
            // Get embed page parser
            var videoEmbedPageParser = await GetVideoEmbedPageParserAsync(videoId).ConfigureAwait(false);

            // Extract info
            var playerSourceUrl = videoEmbedPageParser.GetPlayerSourceUrl();
            var sts = videoEmbedPageParser.GetSts();

            // Check if successful
            if (playerSourceUrl.IsBlank() || sts.IsBlank())
                throw new ParseException("Could not parse player context.");

            return new PlayerContext(playerSourceUrl, sts);
        }

        private async Task<PlayerSourceParser> GetPlayerSourceParserAsync(string sourceUrl)
        {
            var raw = await _httpClient.GetStringAsync(sourceUrl).ConfigureAwait(false);
            return PlayerSourceParser.Initialize(raw);
        }

        private async Task<PlayerSource> GetVideoPlayerSourceAsync(string sourceUrl)
        {
            // Try to resolve from cache first
            var playerSource = _playerSourceCache.GetOrDefault(sourceUrl);
            if (playerSource != null)
                return playerSource;

            // Get player source parser
            var playerSourceParser = await GetPlayerSourceParserAsync(sourceUrl).ConfigureAwait(false);
            
            // Extract cipher operations
            var operations = playerSourceParser.GetCipherOperations();

            return _playerSourceCache[sourceUrl] = new PlayerSource(operations);
        }

        /// <inheritdoc />
        public async Task<Video> GetVideoAsync(string videoId)
        {
            videoId.GuardNotNull(nameof(videoId));

            if (!ValidateVideoId(videoId))
                throw new ArgumentException($"Invalid YouTube video ID [{videoId}].", nameof(videoId));

            // Get video info
            var videoInfo = await GetVideoInfoParserAsync(videoId).ConfigureAwait(false);

            // Extract info
            var title = videoInfo.GetTitle();
            var author = videoInfo.GetAuthor();
            var duration = videoInfo.GetDuration();
            var viewCount = videoInfo.GetViewCount();
            var keywords = videoInfo.GetKeywords();

            // Get video watch page
            var videoWatchPage = await GetVideoWatchPageParserAsync(videoId).ConfigureAwait(false);

            // Extract info
            var uploadDate = videoWatchPage.GetUploadDate();
            var likeCount = videoWatchPage.GetLikeCount();
            var dislikeCount = videoWatchPage.GetDislikeCount();
            var description = videoWatchPage.GetDescription();

            var statistics = new Statistics(viewCount, likeCount, dislikeCount);
            var thumbnails = new ThumbnailSet(videoId);

            return new Video(videoId, author, uploadDate, title, description, thumbnails, duration, keywords,
                statistics);
        }

        /// <inheritdoc />
        public async Task<Channel> GetVideoAuthorChannelAsync(string videoId)
        {
            videoId.GuardNotNull(nameof(videoId));

            if (!ValidateVideoId(videoId))
                throw new ArgumentException($"Invalid YouTube video ID [{videoId}].", nameof(videoId));

            // Get video info just to check error code
            await GetVideoInfoParserAsync(videoId).ConfigureAwait(false);

            // Get embed page
            var videoEmbedPage = await GetVideoEmbedPageParserAsync(videoId).ConfigureAwait(false);

            // Extract info
            var id = videoEmbedPage.GetChannelId();
            var title = videoEmbedPage.GetChannelTitle();
            var logoUrl = videoEmbedPage.GetChannelLogoUrl();

            return new Channel(id, title, logoUrl);
        }

        private async Task<DashManifestParser> GetDashManifestParserAsync(string dashManifestUrl)
        {
            var raw = await _httpClient.GetStringAsync(dashManifestUrl).ConfigureAwait(false);
            return DashManifestParser.Initialize(raw);
        }

        /// <inheritdoc />
        public async Task<MediaStreamInfoSet> GetVideoMediaStreamInfosAsync(string videoId)
        {
            videoId.GuardNotNull(nameof(videoId));

            if (!ValidateVideoId(videoId))
                throw new ArgumentException($"Invalid YouTube video ID [{videoId}].", nameof(videoId));

            // Get player context
            var playerContext = await GetVideoPlayerContextAsync(videoId).ConfigureAwait(false);

            // Get video info parser
            var videoInfoParser = await GetVideoInfoParserAsync(videoId, playerContext.Sts).ConfigureAwait(false);

            // Check if video requires purchase
            var previewVideoId = videoInfoParser.GetPreviewVideoId();
            if (previewVideoId.IsNotBlank())
                throw new VideoRequiresPurchaseException(videoId, previewVideoId);

            // Prepare stream info collections
            var muxedStreamInfoMap = new Dictionary<int, MuxedStreamInfo>();
            var audioStreamInfoMap = new Dictionary<int, AudioStreamInfo>();
            var videoStreamInfoMap = new Dictionary<int, VideoStreamInfo>();

            // Parse muxed stream infos
            foreach (var muxedStreamInfoParser in videoInfoParser.MuxedStreamInfos())
            {
                var itag = muxedStreamInfoParser.GetItag();

#if RELEASE
                if (!ItagHelper.IsKnown(itag))
                    continue;
#endif

                var url = muxedStreamInfoParser.GetUrl();

                // Decipher signature if needed
                var signature = muxedStreamInfoParser.GetSignature();
                if (signature.IsNotBlank())
                {
                    var playerSource =
                        await GetVideoPlayerSourceAsync(playerContext.SourceUrl).ConfigureAwait(false);
                    signature = playerSource.Decipher(signature);
                    url = UrlEx.SetQueryParameter(url, "signature", signature);
                }

                // Probe stream and get content length
                var contentLength = await _httpClient.GetContentLengthAsync(url, false).ConfigureAwait(false) ?? -1;
                
                // If probe failed, the stream is gone or faulty
                if (contentLength < 0)
                    continue;

                var streamInfo = new MuxedStreamInfo(itag, url, contentLength);
                muxedStreamInfoMap[itag] = streamInfo;
            }

            // Parse adaptive stream infos
            foreach (var adaptiveStreamInfoParser in videoInfoParser.AdaptiveStreamInfos())
            {
                var itag = adaptiveStreamInfoParser.GetItag();

#if RELEASE
                if (!ItagHelper.IsKnown(itag))
                    continue;
#endif

                var url = adaptiveStreamInfoParser.GetUrl();
                var contentLength = adaptiveStreamInfoParser.GetContentLength();
                var bitrate = adaptiveStreamInfoParser.GetBitrate();

                // Decipher signature if needed
                var signature = adaptiveStreamInfoParser.GetSignature();
                if (signature.IsNotBlank())
                {
                    var playerSource =
                        await GetVideoPlayerSourceAsync(playerContext.SourceUrl).ConfigureAwait(false);
                    signature = playerSource.Decipher(signature);
                    url = UrlEx.SetQueryParameter(url, "signature", signature);
                }

                // If audio stream
                if (adaptiveStreamInfoParser.GetIsAudioOnly())
                {
                    var streamInfo = new AudioStreamInfo(itag, url, contentLength, bitrate);
                    audioStreamInfoMap[itag] = streamInfo;
                }
                // If video stream
                else
                {
                    // Parse additional data
                    var width = adaptiveStreamInfoParser.GetWidth();
                    var height = adaptiveStreamInfoParser.GetHeight();
                    var resolution = new VideoResolution(width, height);
                    var framerate = adaptiveStreamInfoParser.GetFramerate();
                    var qualityLabel = adaptiveStreamInfoParser.GetQualityLabel();

                    var streamInfo = new VideoStreamInfo(itag, url, contentLength, bitrate, resolution, framerate,
                        qualityLabel);
                    videoStreamInfoMap[itag] = streamInfo;
                }
            }

            // Resolve dash streams
            var dashManifestUrl = videoInfoParser.GetDashManifestUrl();
            if (dashManifestUrl.IsNotBlank())
            {
                // Parse signature
                var signature = Regex.Match(dashManifestUrl, @"/s/(.*?)(?:/|$)").Groups[1].Value;

                // Decipher signature if needed
                if (signature.IsNotBlank())
                {
                    var playerSource = await GetVideoPlayerSourceAsync(playerContext.SourceUrl).ConfigureAwait(false);
                    signature = playerSource.Decipher(signature);
                    dashManifestUrl = UrlEx.SetRouteParameter(dashManifestUrl, "signature", signature);
                }

                // Get the dash manifest parser
                var dashManifestParser = await GetDashManifestParserAsync(dashManifestUrl).ConfigureAwait(false);

                // Parse dash stream infos
                foreach (var dashStreamInfoParser in dashManifestParser.DashStreamInfos())
                {
                    var itag = dashStreamInfoParser.GetItag();

#if RELEASE
                    if (!ItagHelper.IsKnown(itag))
                        continue;
#endif

                    var url = dashStreamInfoParser.GetUrl();
                    var bitrate = dashStreamInfoParser.GetBitrate();
                    var contentLength = dashStreamInfoParser.GetContentLength();

                    // If audio stream
                    if (dashStreamInfoParser.GetIsAudioOnly())
                    {
                        var streamInfo = new AudioStreamInfo(itag, url, contentLength, bitrate);
                        audioStreamInfoMap[itag] = streamInfo;
                    }
                    // If video stream
                    else
                    {
                        // Parse additional data
                        var width = dashStreamInfoParser.GetWidth();
                        var height = dashStreamInfoParser.GetHeight();
                        var resolution = new VideoResolution(width, height);
                        var framerate = dashStreamInfoParser.GetFramerate();

                        var streamInfo = new VideoStreamInfo(itag, url, contentLength, bitrate, resolution, framerate);
                        videoStreamInfoMap[itag] = streamInfo;
                    }
                }
            }

            // Get the raw HLS stream playlist (*.m3u8)
            var hlsPlaylistUrl = videoInfoParser.GetHlsPlaylistUrl();

            // Finalize stream info collections
            var muxedStreamInfos = muxedStreamInfoMap.Values.OrderByDescending(s => s.VideoQuality).ToArray();
            var audioStreamInfos = audioStreamInfoMap.Values.OrderByDescending(s => s.Bitrate).ToArray();
            var videoStreamInfos = videoStreamInfoMap.Values.OrderByDescending(s => s.VideoQuality).ToArray();

            return new MediaStreamInfoSet(muxedStreamInfos, audioStreamInfos, videoStreamInfos, hlsPlaylistUrl);
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<ClosedCaptionTrackInfo>> GetVideoClosedCaptionTrackInfosAsync(string videoId)
        {
            videoId.GuardNotNull(nameof(videoId));

            if (!ValidateVideoId(videoId))
                throw new ArgumentException($"Invalid YouTube video ID [{videoId}].", nameof(videoId));

            // Get video info parser
            var videoInfoParser = await GetVideoInfoParserAsync(videoId).ConfigureAwait(false);

            // Get closed caption track info parsers
            var closedCaptionTrackInfoParsers = videoInfoParser.ClosedCaptionTrackInfos();

            // Parse closed caption track infos
            var closedCaptionTrackInfos = new List<ClosedCaptionTrackInfo>();
            foreach (var closedCaptionTrackInfoParser in closedCaptionTrackInfoParsers)
            {
                // Extract info
                var url = closedCaptionTrackInfoParser.GetUrl();
                var isAutoGenerated = closedCaptionTrackInfoParser.GetIsAutoGenerated();
                var code = closedCaptionTrackInfoParser.GetLanguageCode();
                var name = closedCaptionTrackInfoParser.GetLanguageName();
                var language = new Language(code, name);

                // Enforce format
                url = UrlEx.SetQueryParameter(url, "format", "3");

                var closedCaptionTrackInfo = new ClosedCaptionTrackInfo(url, language, isAutoGenerated);
                closedCaptionTrackInfos.Add(closedCaptionTrackInfo);
            }

            return closedCaptionTrackInfos;
        }
    }
}