using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YoutubeExplode.Internal;
using YoutubeExplode.Internal.Abstractions;
using YoutubeExplode.Internal.Abstractions.CipherOperations;
using YoutubeExplode.Internal.Abstractions.Wrappers.Shared;
using YoutubeExplode.Models;
using YoutubeExplode.Models.ClosedCaptions;
using YoutubeExplode.Models.MediaStreams;

namespace YoutubeExplode
{
    public partial class YoutubeClient
    {
        private readonly Dictionary<string, IReadOnlyList<ICipherOperation>> _cipherOperationsCache =
            new Dictionary<string, IReadOnlyList<ICipherOperation>>();

        /// <inheritdoc />
        public async Task<Video> GetVideoAsync(string videoId)
        {
            videoId.GuardNotNull(nameof(videoId));

            if (!ValidateVideoId(videoId))
                throw new ArgumentException($"Invalid YouTube video ID [{videoId}].", nameof(videoId));

            // Get video AJAX
            var videoAjax = await GetVideoAjaxAsync(videoId);

            // Get player response
            var playerResponse = videoAjax.GetPlayerResponse();

            // Get video watch page
            var videoWatchPage = await GetVideoWatchPageAsync(videoId);

            // Extract info
            var videoAuthor = playerResponse.GetVideoAuthor();
            var videoTitle = playerResponse.GetVideoTitle();
            var videoDuration = playerResponse.GetVideoDuration();
            var videoKeywords = playerResponse.GetVideoKeywords();
            var videoUploadDate = videoWatchPage.GetVideoUploadDate();
            var videoDescription = videoWatchPage.GetVideoDescription();
            var videoViewCount = videoWatchPage.TryGetVideoViewCount() ?? 0;
            var videoLikeCount = videoWatchPage.TryGetVideoLikeCount() ?? 0;
            var videoDislikeCount = videoWatchPage.TryGetVideoDislikeCount() ?? 0;

            var statistics = new Statistics(videoViewCount, videoLikeCount, videoDislikeCount);
            var thumbnails = new ThumbnailSet(videoId);

            return new Video(videoId, videoAuthor, videoUploadDate, videoTitle, videoDescription,
                thumbnails, videoDuration, videoKeywords, statistics);
        }

        /// <inheritdoc />
        public async Task<Channel> GetVideoAuthorChannelAsync(string videoId)
        {
            videoId.GuardNotNull(nameof(videoId));

            if (!ValidateVideoId(videoId))
                throw new ArgumentException($"Invalid YouTube video ID [{videoId}].", nameof(videoId));

            // Get video AJAX
            var videoAjax = await GetVideoAjaxAsync(videoId);

            // Get player response
            var playerResponse = videoAjax.GetPlayerResponse();

            // Get channel ID
            var channelId = playerResponse.GetChannelId();

            // Get channel page
            var channelPage = await GetChannelPageAsync(channelId);

            // Extract info
            var channelTitle = channelPage.GetChannelTitle();
            var channelLogoUrl = channelPage.GetChannelLogoUrl();

            return new Channel(channelId, channelTitle, channelLogoUrl);
        }

        private async Task<string> DecipherSignatureAsync(string playerSourceUrl, string signature)
        {
            // Try to resolve cipher operations from cache
            var cipherOperations = _cipherOperationsCache.GetValueOrDefault(playerSourceUrl);

            // If they are not in cache - retrieve them
            if (cipherOperations == null)
            {
                // Get player source
                var playerSource = await GetPlayerSourceAsync(playerSourceUrl);

                // Extract cipher operations and save to cache
                cipherOperations = playerSource.GetCipherOperations();
                _cipherOperationsCache[playerSourceUrl] = cipherOperations;
            }

            // Execute cipher operations on signature
            foreach (var cipherOperation in cipherOperations)
                signature = cipherOperation.Decipher(signature);

            return signature;
        }

        /// <inheritdoc />
        public async Task<MediaStreamInfoSet> GetVideoMediaStreamInfosAsync(string videoId)
        {
            videoId.GuardNotNull(nameof(videoId));

            if (!ValidateVideoId(videoId))
                throw new ArgumentException($"Invalid YouTube video ID [{videoId}].", nameof(videoId));

            string videoPlayerSourceUrl;
            IEnumerable<StreamInfoUrlEncoded> muxedStreamInfoParsers;
            IEnumerable<StreamInfoUrlEncoded> adaptiveStreamInfoParsers;
            string dashManifestUrl;
            string hlsManifestUrl;

            // Get video watch page
            var videoWatchPage = await GetVideoWatchPageAsync(videoId);

            // If successful - extract stuff
            if (videoWatchPage.TryGetErrorReason().IsNullOrWhiteSpace())
            {
                var playerConfig = videoWatchPage.GetPlayerConfig();

                // Get player source URL
                videoPlayerSourceUrl = playerConfig.GetPlayerSourceUrl();

                // Get stream parsers
                muxedStreamInfoParsers = playerConfig.GetMuxedStreamInfos();
                adaptiveStreamInfoParsers = playerConfig.GetAdaptiveStreamInfos();

                // Get DASH manifest URL
                dashManifestUrl = "";

                // Get HLS manifest URL
                hlsManifestUrl = "";
            }
            else
            {
                var videoEmbedPage = await GetVideoEmbedPageAsync(videoId);
                var playerConfig = videoEmbedPage.GetPlayerConfig();

                var sts = playerConfig.GetSts();
                videoPlayerSourceUrl = playerConfig.GetPlayerSourceUrl();

                //
                var videoAjax = await GetVideoAjaxAsync(videoId, sts);

                muxedStreamInfoParsers = videoAjax.GetMuxedStreamInfos();
                adaptiveStreamInfoParsers = videoAjax.GetAdaptiveStreamInfos();

                dashManifestUrl = videoAjax.TryGetDashManifestUrl();

                hlsManifestUrl = videoAjax.TryGetHlsManifestUrl();
            }

            // Prepare stream info maps
            var muxedStreamInfoMap = new Dictionary<int, MuxedStreamInfo>();
            var audioStreamInfoMap = new Dictionary<int, AudioStreamInfo>();
            var videoStreamInfoMap = new Dictionary<int, VideoStreamInfo>();

            // Parse muxed stream infos
            foreach (var streamInfo in muxedStreamInfoParsers)
            {
                // Parse info
                var itag = streamInfo.GetItag();
                var url = streamInfo.GetUrl();

                // Decipher signature if needed
                var signature = streamInfo.TryGetSignature();
                if (!signature.IsNullOrWhiteSpace())
                {
                    signature = await DecipherSignatureAsync(videoPlayerSourceUrl, signature);
                    url = UrlEx.SetQueryParameter(url, "signature", signature);
                }

                // Try to parse content length, otherwise get it manually
                var contentLength = streamInfo.TryGetContentLength() ?? -1;
                if (contentLength <= 0)
                {
                    // Send HEAD request and get content length
                    contentLength = await _httpClient.GetContentLengthAsync(url, false) ?? -1;

                    // If content length is still not available - stream is gone or faulty
                    if (contentLength <= 0)
                        continue;
                }

                // Parse container
                var containerStr = streamInfo.GetContainer();
                var container = Heuristics.ContainerFromString(containerStr);

                // Parse audio encoding
                var audioEncodingStr = streamInfo.GetAudioEncoding();
                var audioEncoding = Heuristics.AudioEncodingFromString(audioEncodingStr);

                // Parse video encoding
                var videoEncodingStr = streamInfo.GetVideoEncoding();
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
            foreach (var streamInfo in adaptiveStreamInfoParsers)
            {
                // Parse info
                var itag = streamInfo.GetItag();
                var url = streamInfo.GetUrl();
                var bitrate = streamInfo.GetBitrate();

                // Decipher signature if needed
                var signature = streamInfo.TryGetSignature();
                if (!signature.IsNullOrWhiteSpace())
                {
                    signature = await DecipherSignatureAsync(videoPlayerSourceUrl, signature);
                    url = UrlEx.SetQueryParameter(url, "signature", signature);
                }

                // Try to parse content length, otherwise get it manually
                var contentLength = streamInfo.TryGetContentLength() ?? -1;
                if (contentLength <= 0)
                {
                    // Send HEAD request and get content length
                    contentLength = await _httpClient.GetContentLengthAsync(url, false) ?? -1;

                    // If content length is still not available - stream is gone or faulty
                    if (contentLength <= 0)
                        continue;
                }

                // Parse container
                var containerStr = streamInfo.GetContainer();
                var container = Heuristics.ContainerFromString(containerStr);

                // If audio-only
                if (streamInfo.GetIsAudioOnly())
                {
                    // Parse audio encoding
                    var audioEncodingStr = streamInfo.GetAudioEncoding();
                    var audioEncoding = Heuristics.AudioEncodingFromString(audioEncodingStr);

                    // Add stream
                    audioStreamInfoMap[itag] = new AudioStreamInfo(itag, url, container, contentLength, bitrate, audioEncoding);
                }
                // If video-only
                else
                {
                    // Parse video encoding
                    var videoEncodingStr = streamInfo.GetVideoEncoding();
                    var videoEncoding = Heuristics.VideoEncodingFromString(videoEncodingStr);

                    // Parse video quality label and video quality
                    var videoQualityLabel = streamInfo.GetVideoQualityLabel();
                    var videoQuality = Heuristics.VideoQualityFromLabel(videoQualityLabel);

                    // Parse resolution
                    var width = streamInfo.GetWidth();
                    var height = streamInfo.GetHeight();
                    var resolution = new VideoResolution(width, height);

                    // Parse framerate
                    var framerate = streamInfo.GetFramerate();

                    // Add stream
                    videoStreamInfoMap[itag] = new VideoStreamInfo(itag, url, container, contentLength, bitrate, videoEncoding,
                        videoQualityLabel, videoQuality, resolution, framerate);
                }
            }

            // Parse dash manifest
            if (!dashManifestUrl.IsNullOrWhiteSpace())
            {
                // Parse signature
                var signature = Regex.Match(dashManifestUrl, "/s/(.*?)(?:/|$)").Groups[1].Value;

                // Decipher signature if needed
                if (!signature.IsNullOrWhiteSpace())
                {
                    signature = await DecipherSignatureAsync(videoPlayerSourceUrl, signature);
                    dashManifestUrl = UrlEx.SetRouteParameter(dashManifestUrl, "signature", signature);
                }

                // Get the dash manifest parser
                var dashManifestParser = await GetDashManifestAsync(dashManifestUrl);

                // Parse dash stream infos
                foreach (var streamInfo in dashManifestParser.GetStreamInfos())
                {
                    // Parse info
                    var itag = streamInfo.GetItag();
                    var url = streamInfo.GetUrl();
                    var contentLength = streamInfo.GetContentLength();
                    var bitrate = streamInfo.GetBitrate();

                    // Parse container
                    var containerStr = streamInfo.GetContainer();
                    var container = Heuristics.ContainerFromString(containerStr);

                    // If audio-only
                    if (streamInfo.GetIsAudioOnly())
                    {
                        // Parse audio encoding
                        var audioEncodingStr = streamInfo.GetEncoding();
                        var audioEncoding = Heuristics.AudioEncodingFromString(audioEncodingStr);

                        // Add stream
                        audioStreamInfoMap[itag] = new AudioStreamInfo(itag, url, container, contentLength, bitrate, audioEncoding);
                    }
                    // If video-only
                    else
                    {
                        // Parse video encoding
                        var videoEncodingStr = streamInfo.GetEncoding();
                        var videoEncoding = Heuristics.VideoEncodingFromString(videoEncodingStr);

                        // Parse resolution
                        var width = streamInfo.GetWidth();
                        var height = streamInfo.GetHeight();
                        var resolution = new VideoResolution(width, height);

                        // Parse framerate
                        var framerate = streamInfo.GetFramerate();

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

            // Get video AJAX
            var videoAjax = await GetVideoAjaxAsync(videoId);

            // Get player response
            var playerResponse = videoAjax.GetPlayerResponse();

            // Parse closed caption track infos
            var closedCaptionTrackInfos = new List<ClosedCaptionTrackInfo>();
            foreach (var closedCaptionTrackInfoParser in playerResponse.GetClosedCaptionTrackInfos())
            {
                // Parse info
                var url = closedCaptionTrackInfoParser.GetUrl();
                var isAutoGenerated = closedCaptionTrackInfoParser.GetIsAutoGenerated();

                // Parse language
                var languageCode = closedCaptionTrackInfoParser.GetLanguageCode();
                var languageName = closedCaptionTrackInfoParser.GetLanguageName();
                var language = new Language(languageCode, languageName);

                // Enforce format to the one we know how to parse
                url = UrlEx.SetQueryParameter(url, "format", "3");

                var closedCaptionTrackInfo = new ClosedCaptionTrackInfo(url, language, isAutoGenerated);
                closedCaptionTrackInfos.Add(closedCaptionTrackInfo);
            }

            return closedCaptionTrackInfos;
        }
    }
}