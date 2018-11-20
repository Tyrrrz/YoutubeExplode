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
                // Extract itag to uniquely identify this stream
                var itag = streamInfoParser.ParseItag();

                // Extract info
                var url = streamInfoParser.ParseUrl();
                var contentLength = streamInfoParser.ParseContentLength();
                var bitrate = streamInfoParser.ParseBitrate();
                var format = streamInfoParser.ParseFormat();
                var audioCodec = streamInfoParser.ParseAudioCodec();
                var videoCodec = streamInfoParser.ParseVideoCodec();
                var videoQualityLabel = streamInfoParser.ParseVideoQualityLabel();
                var width = streamInfoParser.ParseWidth();
                var height = streamInfoParser.ParseHeight();
                var framerate = streamInfoParser.ParseFramerate();

                // Determine video quality from quality label
                var videoQuality = VideoQualityConverter.VideoQualityFromLabel(videoQualityLabel);


                // If content length is not set - get it manually
                if (contentLength <= 0)
                {
                    // Send HEAD request and get content length
                    contentLength = await _httpClient.GetContentLengthAsync(url, false).ConfigureAwait(false) ?? -1;

                    // If content length is still not available - stream is gone or faulty
                    if (contentLength <= 0)
                        continue;
                }

                // If bitrate is not set - calculate it manually
                if (bitrate <= 0)
                {
                    // Average bitrate = content length divided by duration
                    var duration = streamInfoParser.ParseDuration();
                    bitrate = (long) (0.001 * contentLength / (duration.TotalMinutes * 0.0075));
                }

                var resolution = new VideoResolution(width, height);
                muxedStreamInfoMap[itag] = new MuxedStreamInfo(url, contentLength, bitrate, format, audioCodec,
                    videoCodec, videoQualityLabel, videoQuality, resolution, framerate);
            }

            // Parse adaptive stream infos
            foreach (var streamInfoParser in parser.GetAdaptiveStreamInfos())
            {
                // Extract itag to uniquely identify this stream
                var itag = streamInfoParser.ParseItag();

                // Extract info
                var url = streamInfoParser.ParseUrl();
                var contentLength = streamInfoParser.ParseContentLength();
                var bitrate = streamInfoParser.ParseBitrate();
                var format = streamInfoParser.ParseFormat();

                // If content length is not set - get it manually
                if (contentLength <= 0)
                {
                    // Send HEAD request and get content length
                    contentLength = await _httpClient.GetContentLengthAsync(url, false).ConfigureAwait(false) ?? -1;

                    // If content length is still not available - stream is gone or faulty
                    if (contentLength <= 0)
                        continue;
                }

                // If bitrate is not set - calculate it manually
                if (bitrate <= 0)
                {
                    // Average bitrate = content length divided by duration
                    var duration = streamInfoParser.ParseDuration();
                    bitrate = (long)(0.001 * contentLength / (duration.TotalMinutes * 0.0075));
                }

                // If audio-only
                if (streamInfoParser.ParseIsAudioOnly())
                {
                    // Extract audio-specific info
                    var audioCodec = streamInfoParser.ParseAudioCodec();

                    // Add stream to map
                    audioStreamInfoMap[itag] = new AudioStreamInfo(url, contentLength, bitrate, format, audioCodec);
                }
                // If video-only
                else
                {
                    // Extract video-specific info
                    var videoCodec = streamInfoParser.ParseVideoCodec();
                    var videoQualityLabel = streamInfoParser.ParseVideoQualityLabel();
                    var width = streamInfoParser.ParseWidth();
                    var height = streamInfoParser.ParseHeight();
                    var framerate = streamInfoParser.ParseFramerate();

                    // Determine video quality from quality label
                    var videoQuality = VideoQualityConverter.VideoQualityFromLabel(videoQualityLabel);

                    var resolution = new VideoResolution(width, height);
                    videoStreamInfoMap[itag] = new VideoStreamInfo(url, contentLength, bitrate, format, videoCodec,
                        videoQualityLabel, videoQuality, resolution, framerate);
                }
            }

            // Parse dash manifest
            var dashManifestUrl = parser.ParseDashManifestUrl();
            if (dashManifestUrl.IsNotBlank())
            {
                // Get the dash manifest parser
                var dashManifestParser = await GetDashManifestParserAsync(dashManifestUrl).ConfigureAwait(false);

                // Parse dash stream infos
                foreach (var dashStreamInfoParser in dashManifestParser.GetStreamInfos())
                {
                    // Extract itag to uniquely identify this stream
                    var itag = dashStreamInfoParser.ParseItag();

                    // Extract info
                    var url = dashStreamInfoParser.ParseUrl();
                    var contentLength = dashStreamInfoParser.ParseContentLength();
                    var bitrate = dashStreamInfoParser.ParseBitrate();
                    var format = dashStreamInfoParser.ParseFormat();

                    // If audio-only
                    if (dashStreamInfoParser.ParseIsAudioOnly())
                    {
                        // Extract audio-specific info
                        var audioCodec = dashStreamInfoParser.ParseAudioCodec();

                        // Add stream to map
                        audioStreamInfoMap[itag] = new AudioStreamInfo(url, contentLength, bitrate, format,
                            audioCodec);
                    }
                    // If video-only
                    else
                    {
                        // Extract video-specific info
                        var videoCodec = dashStreamInfoParser.ParseVideoCodec();
                        var width = dashStreamInfoParser.ParseWidth();
                        var height = dashStreamInfoParser.ParseHeight();
                        var framerate = dashStreamInfoParser.ParseFramerate();

                        // Determine video quality from height
                        var videoQuality = VideoQualityConverter.VideoQualityFromHeight(height);

                        // Determine video quality label from video quality and framerate
                        var videoQualityLabel = VideoQualityConverter.VideoQualityToLabel(videoQuality, framerate);

                        var resolution = new VideoResolution(width, height);
                        videoStreamInfoMap[itag] = new VideoStreamInfo(url, contentLength, bitrate, format,
                            videoCodec, videoQualityLabel, videoQuality, resolution, framerate);
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