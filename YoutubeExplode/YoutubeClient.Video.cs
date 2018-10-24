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
            // Get parser for 'el=embedded'
            var parser = await GetVideoInfoParserAsync(videoId, "embedded", sts).ConfigureAwait(false);

            // Check if video exists by verifying that video ID property is not empty
            if (parser.ParseId().IsBlank())
            {
                // Get native error code and error reason
                var errorCode = parser.ParseErrorCode();
                var errorReason = parser.ParseErrorReason();

                throw new VideoUnavailableException(videoId, errorCode, errorReason);
            }

            // If requested with "sts" parameter, it means that the calling code is interested in getting video info with streams.
            // For that we also need to make sure the video is fully available by checking for errors.
            if (sts.IsNotBlank() && parser.ParseErrorCode() != 0)
            {
                // Retry for "el=detailpage"
                parser = await GetVideoInfoParserAsync(videoId, "detailpage", sts).ConfigureAwait(false);

                // If there are still errors - throw
                if (parser.ParseErrorCode() != 0)
                {
                    // Get native error code and error reason
                    var errorCode = parser.ParseErrorCode();
                    var errorReason = parser.ParseErrorReason();

                    throw new VideoUnavailableException(videoId, errorCode, errorReason);
                }
            }

            return parser;
        }

        private async Task<PlayerSourceParser> GetPlayerSourceParserAsync(string sourceUrl)
        {
            var raw = await _httpClient.GetStringAsync(sourceUrl).ConfigureAwait(false);
            return PlayerSourceParser.Initialize(raw);
        }

        private async Task<DashManifestParser> GetDashManifestParserAsync(string dashManifestUrl)
        {
            var raw = await _httpClient.GetStringAsync(dashManifestUrl).ConfigureAwait(false);
            return DashManifestParser.Initialize(raw);
        }

        private async Task<PlayerContext> GetVideoPlayerContextAsync(string videoId)
        {
            // Get parser
            var parser = await GetVideoEmbedPageParserAsync(videoId).ConfigureAwait(false);

            // Extract info
            var playerSourceUrl = parser.ParsePlayerSourceUrl();
            var sts = parser.ParseSts();

            // Check if successful
            if (playerSourceUrl.IsBlank() || sts.IsBlank())
                throw new ParseException("Could not parse player context.");

            return new PlayerContext(playerSourceUrl, sts);
        }

        private async Task<PlayerSource> GetVideoPlayerSourceAsync(string sourceUrl)
        {
            // Try to resolve from cache first
            var playerSource = _playerSourceCache.GetOrDefault(sourceUrl);
            if (playerSource != null)
                return playerSource;

            // Get parser
            var parser = await GetPlayerSourceParserAsync(sourceUrl).ConfigureAwait(false);
            
            // Extract cipher operations
            var operations = parser.ParseCipherOperations();

            return _playerSourceCache[sourceUrl] = new PlayerSource(operations);
        }

        /// <inheritdoc />
        public async Task<Video> GetVideoAsync(string videoId)
        {
            videoId.GuardNotNull(nameof(videoId));

            if (!ValidateVideoId(videoId))
                throw new ArgumentException($"Invalid YouTube video ID [{videoId}].", nameof(videoId));

            // Get video info parser
            var videoInfoParser = await GetVideoInfoParserAsync(videoId).ConfigureAwait(false);

            // Extract info
            var author = videoInfoParser.ParseAuthor();
            var title = videoInfoParser.ParseTitle();
            var duration = videoInfoParser.ParseDuration();
            var keywords = videoInfoParser.ParseKeywords();
            var viewCount = videoInfoParser.ParseViewCount();

            // Get video watch page parser
            var videoWatchPageParser = await GetVideoWatchPageParserAsync(videoId).ConfigureAwait(false);

            // Extract info
            var uploadDate = videoWatchPageParser.ParseUploadDate();
            var description = videoWatchPageParser.ParseDescription();
            var likeCount = videoWatchPageParser.ParseLikeCount();
            var dislikeCount = videoWatchPageParser.ParseDislikeCount();

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

            // Get video info parser just to assert that the video exists
            await GetVideoInfoParserAsync(videoId).ConfigureAwait(false);

            // Get parser
            var parser = await GetVideoEmbedPageParserAsync(videoId).ConfigureAwait(false);

            // Extract info
            var id = parser.ParseChannelId();
            var title = parser.ParseChannelTitle();
            var logoUrl = parser.ParseChannelLogoUrl();

            return new Channel(id, title, logoUrl);
        }

        /// <inheritdoc />
        public async Task<MediaStreamInfoSet> GetVideoMediaStreamInfosAsync(string videoId)
        {
            videoId.GuardNotNull(nameof(videoId));

            if (!ValidateVideoId(videoId))
                throw new ArgumentException($"Invalid YouTube video ID [{videoId}].", nameof(videoId));

            // Get player context
            var playerContext = await GetVideoPlayerContextAsync(videoId).ConfigureAwait(false);

            // Get parser
            var parser = await GetVideoInfoParserAsync(videoId, playerContext.Sts).ConfigureAwait(false);

            // Check if video requires purchase
            var previewVideoId = parser.ParsePreviewVideoId();
            if (previewVideoId.IsNotBlank())
                throw new VideoRequiresPurchaseException(videoId, previewVideoId);

            // Prepare stream info maps
            var muxedStreamInfoMap = new Dictionary<int, MuxedStreamInfo>();
            var audioStreamInfoMap = new Dictionary<int, AudioStreamInfo>();
            var videoStreamInfoMap = new Dictionary<int, VideoStreamInfo>();

            // Parse muxed stream infos
            foreach (var muxedStreamInfoParser in parser.GetMuxedStreamInfos())
            {
                // Extract itag
                var itag = muxedStreamInfoParser.ParseItag();

#if RELEASE
                // Skip unknown itags
                if (!ItagHelper.IsKnown(itag))
                    continue;
#endif

                // Extract URL
                var url = muxedStreamInfoParser.ParseUrl();

                // Decipher signature if needed
                var signature = muxedStreamInfoParser.ParseSignature();
                if (signature.IsNotBlank())
                {
                    var playerSource =
                        await GetVideoPlayerSourceAsync(playerContext.SourceUrl).ConfigureAwait(false);
                    signature = playerSource.Decipher(signature);
                    url = UrlEx.SetQueryParameter(url, "signature", signature);
                }

                // Probe stream and get content length
                var contentLength = await _httpClient.GetContentLengthAsync(url, false).ConfigureAwait(false) ?? -1;
                
                // If probe failed or content length is 0, it means the stream is gone or faulty
                if (contentLength <= 0)
                    continue;

                var streamInfo = new MuxedStreamInfo(itag, url, contentLength);
                muxedStreamInfoMap[itag] = streamInfo;
            }

            // Parse adaptive stream infos
            foreach (var adaptiveStreamInfoParser in parser.GetAdaptiveStreamInfos())
            {
                // Extract info
                var itag = adaptiveStreamInfoParser.ParseItag();

#if RELEASE
                // Skip unknown itags
                if (!ItagHelper.IsKnown(itag))
                    continue;
#endif

                // Extract content length
                var contentLength = adaptiveStreamInfoParser.ParseContentLength();

                // If content length is 0, it means that the stream is gone or faulty
                if (contentLength <= 0)
                    continue;

                // Extract URL
                var url = adaptiveStreamInfoParser.ParseUrl();

                // Decipher signature if needed
                var signature = adaptiveStreamInfoParser.ParseSignature();
                if (signature.IsNotBlank())
                {
                    var playerSource =
                        await GetVideoPlayerSourceAsync(playerContext.SourceUrl).ConfigureAwait(false);
                    signature = playerSource.Decipher(signature);
                    url = UrlEx.SetQueryParameter(url, "signature", signature);
                }

                // Extract bitrate
                var bitrate = adaptiveStreamInfoParser.ParseBitrate();

                // If audio-only
                if (adaptiveStreamInfoParser.ParseIsAudioOnly())
                {
                    var streamInfo = new AudioStreamInfo(itag, url, contentLength, bitrate);
                    audioStreamInfoMap[itag] = streamInfo;
                }
                // If video-only
                else
                {
                    // Extract info
                    var width = adaptiveStreamInfoParser.ParseWidth();
                    var height = adaptiveStreamInfoParser.ParseHeight();
                    var framerate = adaptiveStreamInfoParser.ParseFramerate();
                    var qualityLabel = adaptiveStreamInfoParser.ParseQualityLabel();

                    var resolution = new VideoResolution(width, height);
                    var streamInfo = new VideoStreamInfo(itag, url, contentLength, bitrate, resolution, framerate,
                        qualityLabel);
                    videoStreamInfoMap[itag] = streamInfo;
                }
            }

            // Parse dash manifest
            var dashManifestUrl = parser.ParseDashManifestUrl();
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
                foreach (var dashStreamInfoParser in dashManifestParser.GetStreamInfos())
                {
                    // Extract itag
                    var itag = dashStreamInfoParser.ParseItag();

#if RELEASE
                    // Skip unknown itags
                    if (!ItagHelper.IsKnown(itag))
                        continue;
#endif

                    // Extract info
                    var url = dashStreamInfoParser.ParseUrl();
                    var contentLength = dashStreamInfoParser.ParseContentLength();
                    var bitrate = dashStreamInfoParser.ParseBitrate();

                    // If audio-only
                    if (dashStreamInfoParser.ParseIsAudioOnly())
                    {
                        var streamInfo = new AudioStreamInfo(itag, url, contentLength, bitrate);
                        audioStreamInfoMap[itag] = streamInfo;
                    }
                    // If video-only
                    else
                    {
                        // Parse additional data
                        var width = dashStreamInfoParser.ParseWidth();
                        var height = dashStreamInfoParser.ParseHeight();
                        var framerate = dashStreamInfoParser.ParseFramerate();

                        var resolution = new VideoResolution(width, height);
                        var streamInfo = new VideoStreamInfo(itag, url, contentLength, bitrate, resolution, framerate);
                        videoStreamInfoMap[itag] = streamInfo;
                    }
                }
            }

            // Get the raw HLS stream playlist (*.m3u8)
            var hlsPlaylistUrl = parser.ParseHlsPlaylistUrl();

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
            var parser = await GetVideoInfoParserAsync(videoId).ConfigureAwait(false);

            // Parse closed caption track infos
            var closedCaptionTrackInfos = new List<ClosedCaptionTrackInfo>();
            foreach (var closedCaptionTrackInfoParser in parser.GetClosedCaptionTrackInfos())
            {
                // Extract info
                var url = closedCaptionTrackInfoParser.ParseUrl();
                var code = closedCaptionTrackInfoParser.ParseLanguageCode();
                var name = closedCaptionTrackInfoParser.ParseLanguageName();
                var isAutoGenerated = closedCaptionTrackInfoParser.ParseIsAutoGenerated();

                // Enforce format to the format we know how to parse
                url = UrlEx.SetQueryParameter(url, "format", "3");

                var language = new Language(code, name);
                var closedCaptionTrackInfo = new ClosedCaptionTrackInfo(url, language, isAutoGenerated);
                closedCaptionTrackInfos.Add(closedCaptionTrackInfo);
            }

            return closedCaptionTrackInfos;
        }
    }
}