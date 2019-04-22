using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YoutubeExplode.Exceptions;
using YoutubeExplode.Internal;
using YoutubeExplode.Models;
using YoutubeExplode.Models.ClosedCaptions;
using YoutubeExplode.Models.MediaStreams;

namespace YoutubeExplode
{
    public partial class YoutubeClient
    {
        private readonly Dictionary<string, PlayerSource> _playerSourceCache = new Dictionary<string, PlayerSource>();

        /// <inheritdoc />
        public async Task<Video> GetVideoAsync(string videoId)
        {
            videoId.GuardNotNull(nameof(videoId));

            if (!ValidateVideoId(videoId))
                throw new ArgumentException($"Invalid YouTube video ID [{videoId}].", nameof(videoId));

            // Get video info parser
            var videoInfoParser = await GetVideoInfoParserAsync(videoId);

            // Get player response parser
            var playerResponseParser = videoInfoParser.GetPlayerResponse();

            // Get video watch page parser
            var videoWatchPageParser = await GetVideoWatchPageParserAsync(videoId);

            // Parse info
            var author = playerResponseParser.ParseAuthor();
            var title = playerResponseParser.ParseTitle();
            var duration = playerResponseParser.ParseDuration();
            var keywords = playerResponseParser.ParseKeywords();
            var uploadDate = videoWatchPageParser.ParseUploadDate();
            var description = videoWatchPageParser.ParseDescription();
            var viewCount = videoWatchPageParser.ParseViewCount();
            var likeCount = videoWatchPageParser.ParseLikeCount();
            var dislikeCount = videoWatchPageParser.ParseDislikeCount();

            var statistics = new Statistics(viewCount, likeCount, dislikeCount);
            var thumbnails = new ThumbnailSet(videoId);

            return new Video(videoId, author, uploadDate, title, description, thumbnails, duration, keywords, statistics);
        }

        /// <inheritdoc />
        public async Task<Channel> GetVideoAuthorChannelAsync(string videoId)
        {
            videoId.GuardNotNull(nameof(videoId));

            if (!ValidateVideoId(videoId))
                throw new ArgumentException($"Invalid YouTube video ID [{videoId}].", nameof(videoId));

            // Get video info parser
            var videoInfoParser = await GetVideoInfoParserAsync(videoId);

            // Get player response parser
            var playerResponseParser = videoInfoParser.GetPlayerResponse();

            // Get channel ID
            var id = playerResponseParser.ParseChannelId();

            // Get channel page parser
            var channelPageParser = await GetChannelPageParserAsync(id);

            // Parse info
            var title = channelPageParser.ParseChannelTitle();
            var logoUrl = channelPageParser.ParseChannelLogoUrl();

            return new Channel(id, title, logoUrl);
        }

        private async Task<PlayerSource> GetVideoPlayerSourceAsync(string sourceUrl)
        {
            // Try to resolve from cache first
            var playerSource = _playerSourceCache.GetValueOrDefault(sourceUrl);
            if (playerSource != null)
                return playerSource;

            // Get parser
            var parser = await GetPlayerSourceParserAsync(sourceUrl);

            // Extract cipher operations
            var operations = parser.ParseCipherOperations();

            return _playerSourceCache[sourceUrl] = new PlayerSource(operations);
        }

        /// <inheritdoc />
        public async Task<MediaStreamInfoSet> GetVideoMediaStreamInfosAsync(string videoId)
        {
            videoId.GuardNotNull(nameof(videoId));

            if (!ValidateVideoId(videoId))
                throw new ArgumentException($"Invalid YouTube video ID [{videoId}].", nameof(videoId));

            // TODO
            // Get video watch page parser
            var videoWatchPageParser = await GetVideoWatchPageParserAsync(videoId);
            var videoWatchPageConfigParser = videoWatchPageParser.GetConfig();

            // Get player source URL
            var videoPlayerSourceUrl = videoWatchPageConfigParser.ParsePlayerSourceUrl();

            // Get stream info parsers
            var muxedStreamInfoParsers = videoWatchPageConfigParser.GetMuxedStreamInfos();
            var adaptiveStreamInfoParsers = videoWatchPageConfigParser.GetAdaptiveStreamInfos();

            // If failed - retry with video info
            if (!videoWatchPageParser.ParseErrorReason().IsNullOrWhiteSpace())
            {
                var sts = videoWatchPageConfigParser.ParseSts();

                // Get video info parser
                var videoInfoParser = await GetVideoInfoParserAsync(videoId, sts);

                // Get stream info parsers
                muxedStreamInfoParsers = videoInfoParser.GetMuxedStreamInfos();
                adaptiveStreamInfoParsers = videoInfoParser.GetAdaptiveStreamInfos();
            }

            // If the video requires purchase - throw
            var previewVideoId = videoWatchPageConfigParser.ParsePreviewVideoId();
            if (!previewVideoId.IsNullOrWhiteSpace())
            {
                throw new VideoRequiresPurchaseException(videoId, previewVideoId,
                    $"Video [{videoId}] is unplayable because it requires purchase.");
            }

            // Prepare stream info maps
            var muxedStreamInfoMap = new Dictionary<int, MuxedStreamInfo>();
            var audioStreamInfoMap = new Dictionary<int, AudioStreamInfo>();
            var videoStreamInfoMap = new Dictionary<int, VideoStreamInfo>();

            // Parse muxed stream infos
            foreach (var streamInfoParser in muxedStreamInfoParsers)
            {
                // Parse info
                var itag = streamInfoParser.ParseItag();
                var url = streamInfoParser.ParseUrl();

                // Decipher signature if needed
                var signature = streamInfoParser.ParseSignature();
                if (!signature.IsNullOrWhiteSpace())
                {
                    var playerSource = await GetVideoPlayerSourceAsync(videoPlayerSourceUrl);
                    signature = playerSource.Decipher(signature);
                    url = UrlEx.SetQueryParameter(url, "signature", signature);
                }

                // Try to parse content length, otherwise get it manually
                var contentLength = streamInfoParser.ParseContentLength();
                if (contentLength <= 0)
                {
                    // Send HEAD request and get content length
                    contentLength = await _httpClient.GetContentLengthAsync(url, false) ?? -1;

                    // If content length is still not available - stream is gone or faulty
                    if (contentLength <= 0)
                        continue;
                }

                // Parse container
                var containerStr = streamInfoParser.ParseContainer();
                var container = Heuristics.ContainerFromString(containerStr);

                // Parse audio encoding
                var audioEncodingStr = streamInfoParser.ParseAudioEncoding();
                var audioEncoding = Heuristics.AudioEncodingFromString(audioEncodingStr);

                // Parse video encoding
                var videoEncodingStr = streamInfoParser.ParseVideoEncoding();
                var videoEncoding = Heuristics.VideoEncodingFromString(videoEncodingStr);

                // Determine video quality from itag
                var videoQuality = Heuristics.VideoQualityFromItag(itag);

                // Determine video quality label from video quality
                var videoQualityLabel = Heuristics.VideoQualityToLabel(videoQuality);

                // Determine video resolution from video quality
                var resolution = Heuristics.VideoQualityToResolution(videoQuality);

                // Add stream
                muxedStreamInfoMap[itag] = new MuxedStreamInfo(itag, url, container, contentLength, audioEncoding, videoEncoding,
                    videoQualityLabel, videoQuality, resolution);
            }

            // Parse adaptive stream infos
            foreach (var streamInfoParser in adaptiveStreamInfoParsers)
            {
                // Parse info
                var itag = streamInfoParser.ParseItag();
                var url = streamInfoParser.ParseUrl();
                var bitrate = streamInfoParser.ParseBitrate();

                // Decipher signature if needed
                var signature = streamInfoParser.ParseSignature();
                if (!signature.IsNullOrWhiteSpace())
                {
                    var playerSource = await GetVideoPlayerSourceAsync(videoPlayerSourceUrl);
                    signature = playerSource.Decipher(signature);
                    url = UrlEx.SetQueryParameter(url, "signature", signature);
                }

                // Try to parse content length, otherwise get it manually
                var contentLength = streamInfoParser.ParseContentLength();
                if (contentLength <= 0)
                {
                    // Send HEAD request and get content length
                    contentLength = await _httpClient.GetContentLengthAsync(url, false) ?? -1;

                    // If content length is still not available - stream is gone or faulty
                    if (contentLength <= 0)
                        continue;
                }

                // Parse container
                var containerStr = streamInfoParser.ParseContainer();
                var container = Heuristics.ContainerFromString(containerStr);

                // If audio-only
                if (streamInfoParser.ParseIsAudioOnly())
                {
                    // Parse audio encoding
                    var audioEncodingStr = streamInfoParser.ParseAudioEncoding();
                    var audioEncoding = Heuristics.AudioEncodingFromString(audioEncodingStr);

                    // Add stream
                    audioStreamInfoMap[itag] = new AudioStreamInfo(itag, url, container, contentLength, bitrate, audioEncoding);
                }
                // If video-only
                else
                {
                    // Parse video encoding
                    var videoEncodingStr = streamInfoParser.ParseVideoEncoding();
                    var videoEncoding = Heuristics.VideoEncodingFromString(videoEncodingStr);

                    // Parse video quality label and video quality
                    var videoQualityLabel = streamInfoParser.ParseVideoQualityLabel();
                    var videoQuality = Heuristics.VideoQualityFromLabel(videoQualityLabel);

                    // Parse resolution
                    var width = streamInfoParser.ParseWidth();
                    var height = streamInfoParser.ParseHeight();
                    var resolution = new VideoResolution(width, height);

                    // Parse framerate
                    var framerate = streamInfoParser.ParseFramerate();

                    // Add stream
                    videoStreamInfoMap[itag] = new VideoStreamInfo(itag, url, container, contentLength, bitrate, videoEncoding,
                        videoQualityLabel, videoQuality, resolution, framerate);
                }
            }

            // Parse dash manifest
            var dashManifestUrl = ""; // TODO
            if (!dashManifestUrl.IsNullOrWhiteSpace())
            {
                // Parse signature
                var signature = Regex.Match(dashManifestUrl, "/s/(.*?)(?:/|$)").Groups[1].Value;

                // Decipher signature if needed
                if (!signature.IsNullOrWhiteSpace())
                {
                    var playerSource = await GetVideoPlayerSourceAsync(videoPlayerSourceUrl);
                    signature = playerSource.Decipher(signature);
                    dashManifestUrl = UrlEx.SetRouteParameter(dashManifestUrl, "signature", signature);
                }

                // Get the dash manifest parser
                var dashManifestParser = await GetDashManifestParserAsync(dashManifestUrl);

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
                    var container = Heuristics.ContainerFromString(containerStr);

                    // If audio-only
                    if (streamInfoParser.ParseIsAudioOnly())
                    {
                        // Parse audio encoding
                        var audioEncodingStr = streamInfoParser.ParseEncoding();
                        var audioEncoding = Heuristics.AudioEncodingFromString(audioEncodingStr);

                        // Add stream
                        audioStreamInfoMap[itag] = new AudioStreamInfo(itag, url, container, contentLength, bitrate, audioEncoding);
                    }
                    // If video-only
                    else
                    {
                        // Parse video encoding
                        var videoEncodingStr = streamInfoParser.ParseEncoding();
                        var videoEncoding = Heuristics.VideoEncodingFromString(videoEncodingStr);

                        // Parse resolution
                        var width = streamInfoParser.ParseWidth();
                        var height = streamInfoParser.ParseHeight();
                        var resolution = new VideoResolution(width, height);

                        // Parse framerate
                        var framerate = streamInfoParser.ParseFramerate();

                        // Determine video quality from height
                        var videoQuality = Heuristics.VideoQualityFromHeight(height);

                        // Determine video quality label from video quality and framerate
                        var videoQualityLabel = Heuristics.VideoQualityToLabel(videoQuality, framerate);

                        // Add stream
                        videoStreamInfoMap[itag] = new VideoStreamInfo(itag, url, container, contentLength, bitrate, videoEncoding,
                            videoQualityLabel, videoQuality, resolution, framerate);
                    }
                }
            }

            // Finalize stream info collections
            var muxedStreamInfos = muxedStreamInfoMap.Values.OrderByDescending(s => s.VideoQuality).ToArray();
            var audioStreamInfos = audioStreamInfoMap.Values.OrderByDescending(s => s.Bitrate).ToArray();
            var videoStreamInfos = videoStreamInfoMap.Values.OrderByDescending(s => s.VideoQuality).ToArray();

            // Get the HLS manifest URL if available
            var hlsManifestUrl = ""; // TODO

            // Get expiry date
            // TODO
            var validUntil = DateTimeOffset.Now;

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
            var videoInfoParser = await GetVideoInfoParserAsync(videoId);

            // Get player response parser
            var playerResponseParser = videoInfoParser.GetPlayerResponse();

            // Parse closed caption track infos
            var closedCaptionTrackInfos = new List<ClosedCaptionTrackInfo>();
            foreach (var closedCaptionTrackInfoParser in playerResponseParser.GetClosedCaptionTrackInfos())
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