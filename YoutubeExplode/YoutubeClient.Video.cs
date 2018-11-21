using System;
using System.Collections.Generic;
using System.Linq;
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

        private async Task<VideoInfoParser> GetVideoInfoParserAsync(string videoId, string el)
        {
            // This parameter does magic and a lot of videos don't work without it
            var eurl = $"https://youtube.googleapis.com/v/{videoId}".UrlEncode();

            var url = $"https://www.youtube.com/get_video_info?video_id={videoId}&el={el}&eurl={eurl}&hl=en";
            var raw = await _httpClient.GetStringAsync(url).ConfigureAwait(false);
            
            return VideoInfoParser.Initialize(raw);
        }

        private async Task<VideoInfoParser> GetVideoInfoParserAsync(string videoId, bool ensureIsPlayable = false)
        {
            // Get parser with 'el=embedded'
            var parser = await GetVideoInfoParserAsync(videoId, "embedded").ConfigureAwait(false);

            // If the video is not available - throw exception
            if (!parser.ParseIsAvailable())
            {
                var errorReason = parser.ParseErrorReason();
                throw new VideoUnavailableException(videoId, $"Video [{videoId}] is unavailable. {errorReason}");
            }

            // If requested to ensure playability but the video is not playable - try again
            if (ensureIsPlayable && !parser.ParseIsPlayable())
            {
                // Retry with "el=detailpage"
                parser = await GetVideoInfoParserAsync(videoId, "detailpage").ConfigureAwait(false);

                // If the video is still not playable - throw exception
                if (!parser.ParseIsPlayable())
                {
                    var errorReason = parser.ParseErrorReason();
                    throw new VideoUnplayableException(videoId, $"Video [{videoId}] is unplayable. {errorReason}");
                }
            }

            return parser;
        }

        private async Task<DashManifestParser> GetDashManifestParserAsync(string dashManifestUrl)
        {
            var raw = await _httpClient.GetStringAsync(dashManifestUrl).ConfigureAwait(false);
            return DashManifestParser.Initialize(raw);
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

            // Register the time at which the request was made to calculate expiry date later on
            var requestedAt = DateTimeOffset.Now;

            // Get parser
            var parser = await GetVideoInfoParserAsync(videoId, true).ConfigureAwait(false);

            // Prepare stream info maps
            var muxedStreamInfoMap = new Dictionary<int, MuxedStreamInfo>();
            var audioStreamInfoMap = new Dictionary<int, AudioStreamInfo>();
            var videoStreamInfoMap = new Dictionary<int, VideoStreamInfo>();

            // Parse muxed stream infos
            foreach (var streamInfoParser in parser.GetMuxedStreamInfos())
            {
                // Parse info
                var itag = streamInfoParser.ParseItag();
                var url = streamInfoParser.ParseUrl();

                // Try to parse content length, otherwise get it manually
                var contentLength = streamInfoParser.ParseContentLength();
                if (contentLength <= 0)
                {
                    // Send HEAD request and get content length
                    contentLength = await _httpClient.GetContentLengthAsync(url, false).ConfigureAwait(false) ?? -1;

                    // If content length is still not available - stream is gone or faulty
                    if (contentLength <= 0) continue;
                }

                // Try to parse bitrate, otherwise calculate it
                var bitrate = streamInfoParser.ParseBitrate();
                if (bitrate <= 0)
                {
                    // Average bitrate = content length divided by duration
                    var duration = streamInfoParser.ParseDuration();
                    bitrate = BitrateHelper.CalculateAverageBitrate(contentLength, duration);
                }

                // Parse container
                var containerStr = streamInfoParser.ParseContainer();
                var container = ContainerConverter.ContainerFromString(containerStr);

                // Parse audio encoding
                var audioEncodingStr = streamInfoParser.ParseAudioEncoding();
                var audioEncoding = AudioEncodingConverter.AudioEncodingFromString(audioEncodingStr);

                // Parse video encoding
                var videoEncodingStr = streamInfoParser.ParseVideoEncoding();
                var videoEncoding = VideoEncodingConverter.VideoEncodingFromString(videoEncodingStr);

                // Parse video quality label and video quality
                var videoQualityLabel = streamInfoParser.ParseVideoQualityLabel();
                var videoQuality = VideoQualityConverter.VideoQualityFromLabel(videoQualityLabel);

                // Parse resolution
                var width = streamInfoParser.ParseWidth();
                var height = streamInfoParser.ParseHeight();
                var resolution = new VideoResolution(width, height);

                // Try to parse framerate or just set a dummy one
                var framerate = streamInfoParser.ParseFramerate();
                if (framerate <= 0)
                {
                    // We can only guess
                    framerate = 25; // the most common framerate for muxed streams
                }

                // Add stream
                muxedStreamInfoMap[itag] = new MuxedStreamInfo(itag, url, container, contentLength, bitrate,
                    audioEncoding, videoEncoding, videoQualityLabel, videoQuality, resolution, framerate);
            }

            // Parse adaptive stream infos
            foreach (var streamInfoParser in parser.GetAdaptiveStreamInfos())
            {
                // Parse info
                var itag = streamInfoParser.ParseItag();
                var url = streamInfoParser.ParseUrl();

                // Try to parse content length, otherwise get it manually
                var contentLength = streamInfoParser.ParseContentLength();
                if (contentLength <= 0)
                {
                    // Send HEAD request and get content length
                    contentLength = await _httpClient.GetContentLengthAsync(url, false).ConfigureAwait(false) ?? -1;

                    // If content length is still not available - stream is gone or faulty
                    if (contentLength <= 0) continue;
                }

                // Try to parse bitrate, otherwise calculate it
                var bitrate = streamInfoParser.ParseBitrate();
                if (bitrate <= 0)
                {
                    // Average bitrate = content length divided by duration
                    var duration = streamInfoParser.ParseDuration();
                    bitrate = BitrateHelper.CalculateAverageBitrate(contentLength, duration);
                }

                // Parse container
                var containerStr = streamInfoParser.ParseContainer();
                var container = ContainerConverter.ContainerFromString(containerStr);

                // If audio-only
                if (streamInfoParser.ParseIsAudioOnly())
                {
                    // Parse audio encoding
                    var audioEncodingStr = streamInfoParser.ParseAudioEncoding();
                    var audioEncoding = AudioEncodingConverter.AudioEncodingFromString(audioEncodingStr);

                    // Add stream
                    audioStreamInfoMap[itag] =
                        new AudioStreamInfo(itag, url, container, contentLength, bitrate, audioEncoding);
                }
                // If video-only
                else
                {
                    // Parse video encoding
                    var videoEncodingStr = streamInfoParser.ParseVideoEncoding();
                    var videoEncoding = VideoEncodingConverter.VideoEncodingFromString(videoEncodingStr);

                    // Parse video quality label and video quality
                    var videoQualityLabel = streamInfoParser.ParseVideoQualityLabel();
                    var videoQuality = VideoQualityConverter.VideoQualityFromLabel(videoQualityLabel);

                    // Parse resolution
                    var width = streamInfoParser.ParseWidth();
                    var height = streamInfoParser.ParseHeight();
                    var resolution = new VideoResolution(width, height);

                    // Parse framerate
                    var framerate = streamInfoParser.ParseFramerate();

                    // Add stream
                    videoStreamInfoMap[itag] = new VideoStreamInfo(itag, url, container, contentLength, bitrate,
                        videoEncoding, videoQualityLabel, videoQuality, resolution, framerate);
                }
            }

            // Parse dash manifest
            var dashManifestUrl = parser.ParseDashManifestUrl();
            if (dashManifestUrl.IsNotBlank())
            {
                // Get the dash manifest parser
                var dashManifestParser = await GetDashManifestParserAsync(dashManifestUrl).ConfigureAwait(false);

                // Parse dash stream infos
                foreach (var streamInfoParser in dashManifestParser.GetStreamInfos())
                {
                    // Parse info
                    var itag = streamInfoParser.ParseItag();
                    var url = streamInfoParser.ParseUrl();
                    var contentLength = streamInfoParser.ParseContentLength();
                    var bitrate = streamInfoParser.ParseBitrate();

                    // Parse container
                    var containerStr = streamInfoParser.ParseContainer();
                    var container = ContainerConverter.ContainerFromString(containerStr);

                    // If audio-only
                    if (streamInfoParser.ParseIsAudioOnly())
                    {
                        // Parse audio encoding
                        var audioEncodingStr = streamInfoParser.ParseEncoding();
                        var audioEncoding = AudioEncodingConverter.AudioEncodingFromString(audioEncodingStr);

                        // Add stream
                        audioStreamInfoMap[itag] =
                            new AudioStreamInfo(itag, url, container, contentLength, bitrate, audioEncoding);
                    }
                    // If video-only
                    else
                    {
                        // Parse video encoding
                        var videoEncodingStr = streamInfoParser.ParseEncoding();
                        var videoEncoding = VideoEncodingConverter.VideoEncodingFromString(videoEncodingStr);

                        // Parse resolution
                        var width = streamInfoParser.ParseWidth();
                        var height = streamInfoParser.ParseHeight();
                        var resolution = new VideoResolution(width, height);

                        // Parse framerate
                        var framerate = streamInfoParser.ParseFramerate();

                        // Determine video quality from height
                        var videoQuality = VideoQualityConverter.VideoQualityFromHeight(height);

                        // Determine video quality label from video quality and framerate
                        var videoQualityLabel = VideoQualityConverter.VideoQualityToLabel(videoQuality, framerate);

                        // Add stream
                        videoStreamInfoMap[itag] = new VideoStreamInfo(itag, url, container, contentLength, bitrate,
                            videoEncoding, videoQualityLabel, videoQuality, resolution, framerate);
                    }
                }
            }

            // Finalize stream info collections
            var muxedStreamInfos = muxedStreamInfoMap.Values.OrderByDescending(s => s.VideoQuality).ToArray();
            var audioStreamInfos = audioStreamInfoMap.Values.OrderByDescending(s => s.Bitrate).ToArray();
            var videoStreamInfos = videoStreamInfoMap.Values.OrderByDescending(s => s.VideoQuality).ToArray();

            // Get the HLS manifest URL if available
            var hlsManifestUrl = parser.ParseHlsManifestUrl();

            // Get expiry date
            var lifeSpan = parser.ParseStreamInfoSetLifeSpan();
            var validUntil = requestedAt.Add(lifeSpan);

            return new MediaStreamInfoSet(muxedStreamInfos, audioStreamInfos, videoStreamInfos, hlsManifestUrl,
                validUntil);
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

                // Enforce format to the one we know how to parse
                url = UrlEx.SetQueryParameter(url, "format", "3");

                var language = new Language(code, name);
                var closedCaptionTrackInfo = new ClosedCaptionTrackInfo(url, language, isAutoGenerated);
                closedCaptionTrackInfos.Add(closedCaptionTrackInfo);
            }

            return closedCaptionTrackInfos;
        }
    }
}