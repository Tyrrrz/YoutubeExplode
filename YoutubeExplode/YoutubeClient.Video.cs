using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YoutubeExplode.Exceptions;
using YoutubeExplode.Internal;
using YoutubeExplode.Internal.CipherOperations;
using YoutubeExplode.Internal.Decoders;
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

            // Get video info decoder
            var videoInfoDecoder = await GetVideoInfoDecoderAsync(videoId);

            // Get video watch page decoder
            var videoWatchPageDecoder = await GetVideoWatchPageDecoderAsync(videoId);

            // Extract info
            var videoAuthor = videoInfoDecoder.GetVideoAuthor();
            var videoTitle = videoInfoDecoder.GetVideoTitle();
            var videoDuration = videoInfoDecoder.GetVideoDuration();
            var videoKeywords = videoInfoDecoder.GetVideoKeywords();
            var videoUploadDate = videoWatchPageDecoder.GetVideoUploadDate();
            var videoDescription = videoWatchPageDecoder.GetVideoDescription();
            var videoViewCount = videoWatchPageDecoder.TryGetVideoViewCount() ?? 0;
            var videoLikeCount = videoWatchPageDecoder.TryGetVideoLikeCount() ?? 0;
            var videoDislikeCount = videoWatchPageDecoder.TryGetVideoDislikeCount() ?? 0;

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

            // Get video info decoder
            var videoInfoDecoder = await GetVideoInfoDecoderAsync(videoId);

            // Extract channel ID
            var channelId = videoInfoDecoder.GetChannelId();

            // Get channel page decoder
            var channelPageDecoder = await GetChannelPageDecoderAsync(channelId);

            // Extract info
            var channelTitle = channelPageDecoder.GetChannelTitle();
            var channelLogoUrl = channelPageDecoder.GetChannelLogoUrl();

            return new Channel(channelId, channelTitle, channelLogoUrl);
        }

        private async Task<string> DecipherSignatureAsync(string playerSourceUrl, string signature)
        {
            // Try to resolve cipher operations from cache
            var cipherOperations = _cipherOperationsCache.GetValueOrDefault(playerSourceUrl);

            // If they are not in cache - retrieve them
            if (cipherOperations == null)
            {
                // Get player source decoder
                var playerSourceDecoder = await GetPlayerSourceDecoderAsync(playerSourceUrl);

                // Extract cipher operations and save to cache
                cipherOperations = playerSourceDecoder.GetCipherOperations();
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

            // Record the time this data was requested to calculate expiry date later
            var requestedAt = DateTimeOffset.Now;

            // Get video embed page decoder
            var videoEmbedPageDecoder = await GetVideoEmbedPageDecoderAsync(videoId);

            // Extract sts and player source URL
            var sts = videoEmbedPageDecoder.GetSts();
            var playerSourceUrl = videoEmbedPageDecoder.GetPlayerSourceUrl();

            // Get video info decoder
            var videoInfoDecoder = await GetVideoInfoDecoderAsync(videoId, sts);

            // If video requires purchase - throw
            var previewVideoId = videoInfoDecoder.TryGetPreviewVideoId();
            if (!previewVideoId.IsNullOrWhiteSpace())
            {
                throw new VideoRequiresPurchaseException(videoId, previewVideoId,
                    $"Video [{videoId}] is unplayable because it requires purchase.");
            }

            IReadOnlyList<StreamInfoDecoder> muxedStreamInfoDecoders;
            IReadOnlyList<StreamInfoDecoder> adaptiveStreamInfoDecoders;
            string dashManifestUrl;
            string hlsManifestUrl;
            DateTimeOffset validUntil;

            var errorReason = videoInfoDecoder.TryGetErrorReason();

            if (errorReason.IsNullOrWhiteSpace())
            {
                // Extract stuff
                muxedStreamInfoDecoders = videoInfoDecoder.GetMuxedStreamInfos();
                adaptiveStreamInfoDecoders = videoInfoDecoder.GetAdaptiveStreamInfos();
                dashManifestUrl = videoInfoDecoder.TryGetDashManifestUrl();
                hlsManifestUrl = videoInfoDecoder.TryGetHlsManifestUrl();
                validUntil = requestedAt + videoInfoDecoder.GetExpiresIn();
            }
            // If video is unplayable - resolve using watch page
            else
            {
                // Get video watch page decoder
                var videoWatchPageDecoder = await GetVideoWatchPageDecoderAsync(videoId);

                // If video is still unplayable - throw
                if (!videoWatchPageDecoder.TryGetErrorReason().IsNullOrWhiteSpace())
                {
                    throw new VideoUnplayableException(videoId,
                        $"Video [{videoId}] is unplayable. Reason: {errorReason}");
                }

                // Get stream info decoders
                muxedStreamInfoDecoders = videoWatchPageDecoder.GetMuxedStreamInfos();
                adaptiveStreamInfoDecoders = videoWatchPageDecoder.GetAdaptiveStreamInfos();

                // Extract DASH and HLS manifest URLs
                dashManifestUrl = videoWatchPageDecoder.TryGetDashManifestUrl();
                hlsManifestUrl = videoWatchPageDecoder.TryGetHlsManifestUrl();

                // Extract expiration
                validUntil = requestedAt + videoWatchPageDecoder.GetExpiresIn();
            }

            // Prepare stream info maps
            var muxedStreamInfoMap = new Dictionary<int, MuxedStreamInfo>();
            var audioStreamInfoMap = new Dictionary<int, AudioStreamInfo>();
            var videoStreamInfoMap = new Dictionary<int, VideoStreamInfo>();

            // Parse muxed stream infos
            foreach (var streamInfo in muxedStreamInfoDecoders)
            {
                // Parse info
                var itag = streamInfo.GetItag();
                var url = streamInfo.GetUrl();

                // Decipher signature if needed
                var signature = streamInfo.TryGetSignature();
                if (!signature.IsNullOrWhiteSpace())
                {
                    signature = await DecipherSignatureAsync(playerSourceUrl, signature);
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
            foreach (var streamInfo in adaptiveStreamInfoDecoders)
            {
                // Parse info
                var itag = streamInfo.GetItag();
                var url = streamInfo.GetUrl();
                var bitrate = streamInfo.GetBitrate();

                // Decipher signature if needed
                var signature = streamInfo.TryGetSignature();
                if (!signature.IsNullOrWhiteSpace())
                {
                    signature = await DecipherSignatureAsync(playerSourceUrl, signature);
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
                    signature = await DecipherSignatureAsync(playerSourceUrl, signature);
                    dashManifestUrl = UrlEx.SetRouteParameter(dashManifestUrl, "signature", signature);
                }

                // Get the dash manifest parser
                var dashManifestParser = await GetDashManifestDecoderAsync(dashManifestUrl);

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

            return new MediaStreamInfoSet(muxedStreamInfos, audioStreamInfos, videoStreamInfos, hlsManifestUrl, validUntil);
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<ClosedCaptionTrackInfo>> GetVideoClosedCaptionTrackInfosAsync(string videoId)
        {
            videoId.GuardNotNull(nameof(videoId));

            if (!ValidateVideoId(videoId))
                throw new ArgumentException($"Invalid YouTube video ID [{videoId}].", nameof(videoId));

            // Get video info decoder
            var videoInfoDecoder = await GetVideoInfoDecoderAsync(videoId);

            // Extract closed caption track infos
            var closedCaptionTrackInfos = new List<ClosedCaptionTrackInfo>();
            foreach (var closedCaptionTrackInfoDecoder in videoInfoDecoder.GetClosedCaptionTrackInfos())
            {
                // Extract info
                var url = closedCaptionTrackInfoDecoder.GetUrl();
                var isAutoGenerated = closedCaptionTrackInfoDecoder.GetIsAutoGenerated();

                // Extract language
                var languageCode = closedCaptionTrackInfoDecoder.GetLanguageCode();
                var languageName = closedCaptionTrackInfoDecoder.GetLanguageName();
                var language = new Language(languageCode, languageName);

                // Enforce format to the one we know how to deal with
                url = UrlEx.SetQueryParameter(url, "format", "3");

                // Add to list
                closedCaptionTrackInfos.Add(new ClosedCaptionTrackInfo(url, language, isAutoGenerated));
            }

            return closedCaptionTrackInfos;
        }
    }
}