using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YoutubeExplode.Internal;
using YoutubeExplode.Internal.Helpers;
using YoutubeExplode.Models;
using YoutubeExplode.Models.ClosedCaptions;
using YoutubeExplode.Models.MediaStreams;

namespace YoutubeExplode
{
    public partial class YoutubeClient
    {
        /// <inheritdoc />
        public async Task<Video> GetVideoAsync(string videoId)
        {
            videoId.GuardNotNull(nameof(videoId));

            if (!ValidateVideoId(videoId))
                throw new ArgumentException($"Invalid YouTube video ID [{videoId}].", nameof(videoId));

            // Get player response parser
            var playerResponseParser = await GetPlayerResponseParserAsync(videoId).ConfigureAwait(false);

            // Parse info
            var author = playerResponseParser.ParseAuthor();
            var title = playerResponseParser.ParseTitle();
            var duration = playerResponseParser.ParseDuration();
            var keywords = playerResponseParser.ParseKeywords();

            // Get video watch page parser
            var videoWatchPageParser = await GetVideoWatchPageParserAsync(videoId).ConfigureAwait(false);

            // Parse info
            var uploadDate = videoWatchPageParser.ParseUploadDate();
            var description = videoWatchPageParser.ParseDescription();
            var viewCount = videoWatchPageParser.ParseViewCount();
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

            // Get player response parser
            var playerResponseParser = await GetPlayerResponseParserAsync(videoId).ConfigureAwait(false);

            // Get channel ID
            var id = playerResponseParser.ParseChannelId();

            // Get channel page parser
            var channelPageParser = await GetChannelPageParserAsync(id).ConfigureAwait(false);

            // Parse info
            var title = channelPageParser.ParseChannelTitle();
            var logoUrl = channelPageParser.ParseChannelLogoUrl();

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
            var parser = await GetPlayerResponseParserAsync(videoId, true).ConfigureAwait(false);

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

                // Parse container
                var containerStr = streamInfoParser.ParseContainer();
                var container = ContainerHelper.ContainerFromString(containerStr);

                // Parse audio encoding
                var audioEncodingStr = streamInfoParser.ParseAudioEncoding();
                var audioEncoding = AudioEncodingHelper.AudioEncodingFromString(audioEncodingStr);

                // Parse video encoding
                var videoEncodingStr = streamInfoParser.ParseVideoEncoding();
                var videoEncoding = VideoEncodingHelper.VideoEncodingFromString(videoEncodingStr);

                // Parse video quality label and video quality
                var videoQualityLabel = streamInfoParser.ParseVideoQualityLabel();
                var videoQuality = VideoQualityHelper.VideoQualityFromLabel(videoQualityLabel);

                // Parse resolution
                var width = streamInfoParser.ParseWidth();
                var height = streamInfoParser.ParseHeight();
                var resolution = new VideoResolution(width, height);

                // Add stream
                var streamInfo = new MuxedStreamInfo(itag, url, container, contentLength, audioEncoding, videoEncoding,
                    videoQualityLabel, videoQuality, resolution);
                muxedStreamInfoMap[itag] = streamInfo;
            }

            // Parse adaptive stream infos
            foreach (var streamInfoParser in parser.GetAdaptiveStreamInfos())
            {
                // Parse info
                var itag = streamInfoParser.ParseItag();
                var url = streamInfoParser.ParseUrl();
                var bitrate = streamInfoParser.ParseBitrate();

                // Try to parse content length, otherwise get it manually
                var contentLength = streamInfoParser.ParseContentLength();
                if (contentLength <= 0)
                {
                    // Send HEAD request and get content length
                    contentLength = await _httpClient.GetContentLengthAsync(url, false).ConfigureAwait(false) ?? -1;

                    // If content length is still not available - stream is gone or faulty
                    if (contentLength <= 0) continue;
                }

                // Parse container
                var containerStr = streamInfoParser.ParseContainer();
                var container = ContainerHelper.ContainerFromString(containerStr);

                // If audio-only
                if (streamInfoParser.ParseIsAudioOnly())
                {
                    // Parse audio encoding
                    var audioEncodingStr = streamInfoParser.ParseAudioEncoding();
                    var audioEncoding = AudioEncodingHelper.AudioEncodingFromString(audioEncodingStr);

                    // Add stream
                    var streamInfo = new AudioStreamInfo(itag, url, container, contentLength, bitrate, audioEncoding);
                    audioStreamInfoMap[itag] = streamInfo;
                }
                // If video-only
                else
                {
                    // Parse video encoding
                    var videoEncodingStr = streamInfoParser.ParseVideoEncoding();
                    var videoEncoding = VideoEncodingHelper.VideoEncodingFromString(videoEncodingStr);

                    // Parse video quality label and video quality
                    var videoQualityLabel = streamInfoParser.ParseVideoQualityLabel();
                    var videoQuality = VideoQualityHelper.VideoQualityFromLabel(videoQualityLabel);

                    // Parse resolution
                    var width = streamInfoParser.ParseWidth();
                    var height = streamInfoParser.ParseHeight();
                    var resolution = new VideoResolution(width, height);

                    // Parse framerate
                    var framerate = streamInfoParser.ParseFramerate();

                    // Add stream
                    var streamInfo = new VideoStreamInfo(itag, url, container, contentLength, bitrate, videoEncoding,
                        videoQualityLabel, videoQuality, resolution, framerate);
                    videoStreamInfoMap[itag] = streamInfo;
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
                    var container = ContainerHelper.ContainerFromString(containerStr);

                    // If audio-only
                    if (streamInfoParser.ParseIsAudioOnly())
                    {
                        // Parse audio encoding
                        var audioEncodingStr = streamInfoParser.ParseEncoding();
                        var audioEncoding = AudioEncodingHelper.AudioEncodingFromString(audioEncodingStr);

                        // Add stream
                        var streamInfo =
                            new AudioStreamInfo(itag, url, container, contentLength, bitrate, audioEncoding);
                        audioStreamInfoMap[itag] = streamInfo;
                    }
                    // If video-only
                    else
                    {
                        // Parse video encoding
                        var videoEncodingStr = streamInfoParser.ParseEncoding();
                        var videoEncoding = VideoEncodingHelper.VideoEncodingFromString(videoEncodingStr);

                        // Parse resolution
                        var width = streamInfoParser.ParseWidth();
                        var height = streamInfoParser.ParseHeight();
                        var resolution = new VideoResolution(width, height);

                        // Parse framerate
                        var framerate = streamInfoParser.ParseFramerate();

                        // Determine video quality from height
                        var videoQuality = VideoQualityHelper.VideoQualityFromHeight(height);

                        // Determine video quality label from video quality and framerate
                        var videoQualityLabel = VideoQualityHelper.VideoQualityToLabel(videoQuality, framerate);

                        // Add stream
                        var streamInfo = new VideoStreamInfo(itag, url, container, contentLength, bitrate,
                            videoEncoding, videoQualityLabel, videoQuality, resolution, framerate);
                        videoStreamInfoMap[itag] = streamInfo;
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
            var expiresIn = parser.ParseStreamInfoSetExpiresIn();
            var validUntil = requestedAt.Add(expiresIn);

            return new MediaStreamInfoSet(muxedStreamInfos, audioStreamInfos, videoStreamInfos, hlsManifestUrl,
                validUntil);
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<ClosedCaptionTrackInfo>> GetVideoClosedCaptionTrackInfosAsync(string videoId)
        {
            videoId.GuardNotNull(nameof(videoId));

            if (!ValidateVideoId(videoId))
                throw new ArgumentException($"Invalid YouTube video ID [{videoId}].", nameof(videoId));

            // Get parser
            var parser = await GetPlayerResponseParserAsync(videoId).ConfigureAwait(false);

            // Parse closed caption track infos
            var closedCaptionTrackInfos = new List<ClosedCaptionTrackInfo>();
            foreach (var closedCaptionTrackInfoParser in parser.GetClosedCaptionTrackInfos())
            {
                // Parse info
                var url = closedCaptionTrackInfoParser.ParseUrl();
                var isAutoGenerated = closedCaptionTrackInfoParser.ParseIsAutoGenerated();

                // Parse language
                var code = closedCaptionTrackInfoParser.ParseLanguageCode();
                var name = closedCaptionTrackInfoParser.ParseLanguageName();
                var language = new Language(code, name);

                // Enforce format to the one we know how to parse
                url = UrlEx.SetQueryParameter(url, "format", "3");

                var closedCaptionTrackInfo = new ClosedCaptionTrackInfo(url, language, isAutoGenerated);
                closedCaptionTrackInfos.Add(closedCaptionTrackInfo);
            }

            return closedCaptionTrackInfos;
        }
    }
}