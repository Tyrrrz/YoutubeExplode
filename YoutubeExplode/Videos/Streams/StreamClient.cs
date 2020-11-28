using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using YoutubeExplode.Exceptions;
using YoutubeExplode.Internal;
using YoutubeExplode.Internal.Extensions;
using YoutubeExplode.ReverseEngineering;
using YoutubeExplode.ReverseEngineering.Cipher;
using YoutubeExplode.ReverseEngineering.Responses;

namespace YoutubeExplode.Videos.Streams
{
    /// <summary>
    /// Queries related to media streams of YouTube videos.
    /// </summary>
    public partial class StreamClient
    {
        private readonly YoutubeHttpClient _httpClient;

        /// <summary>
        /// Initializes an instance of <see cref="StreamClient"/>.
        /// </summary>
        internal StreamClient(YoutubeHttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        private async Task<DashManifest> GetDashManifestAsync(
            string dashManifestUrl,
            IReadOnlyList<ICipherOperation> cipherOperations)
        {
            var signature = DashManifest.TryGetSignatureFromUrl(dashManifestUrl);

            if (!string.IsNullOrWhiteSpace(signature))
            {
                signature = cipherOperations.Decipher(signature);
                dashManifestUrl = Url.SetRouteParameter(dashManifestUrl, "signature", signature);
            }

            return await DashManifest.GetAsync(_httpClient, dashManifestUrl);
        }

        private async Task<StreamContext> GetSteamContextFromVideoInfoAsync(VideoId videoId)
        {
            var embedPage = await EmbedPage.GetAsync(_httpClient, videoId);
            var playerConfig =
                embedPage.TryGetPlayerConfig() ??
                throw VideoUnplayableException.Unplayable(videoId);

            var playerSourceUrl = embedPage.TryGetPlayerSourceUrl() ?? playerConfig.GetPlayerSourceUrl();
            var playerSource = await PlayerSource.GetAsync(_httpClient, playerSourceUrl);
            var cipherOperations = playerSource.GetCipherOperations().ToArray();

            var videoInfoResponse = await VideoInfoResponse.GetAsync(_httpClient, videoId, playerSource.GetSts());
            var playerResponse = videoInfoResponse.GetPlayerResponse();

            var previewVideoId = playerResponse.TryGetPreviewVideoId();
            if (!string.IsNullOrWhiteSpace(previewVideoId))
                throw VideoRequiresPurchaseException.Preview(videoId, previewVideoId);

            if (!playerResponse.IsVideoPlayable())
                throw VideoUnplayableException.Unplayable(videoId, playerResponse.TryGetVideoPlayabilityError());

            if (playerResponse.IsLive())
                throw VideoUnplayableException.LiveStream(videoId);

            var streamInfoProviders = new List<IStreamInfoProvider>();

            // Streams from video info
            streamInfoProviders.AddRange(videoInfoResponse.GetStreams());

            // Streams from player response
            streamInfoProviders.AddRange(playerResponse.GetStreams());

            // Streams from DASH manifest
            var dashManifestUrl = playerResponse.TryGetDashManifestUrl();
            if (!string.IsNullOrWhiteSpace(dashManifestUrl))
            {
                var dashManifest = await GetDashManifestAsync(dashManifestUrl, cipherOperations);
                streamInfoProviders.AddRange(dashManifest.GetStreams());
            }

            return new StreamContext(streamInfoProviders, cipherOperations);
        }

        private async Task<StreamContext> GetStreamContextFromWatchPageAsync(VideoId videoId)
        {
            var watchPage = await WatchPage.GetAsync(_httpClient, videoId);
            var playerConfig = watchPage.TryGetPlayerConfig();

            var playerResponse =
                playerConfig?.GetPlayerResponse() ??
                watchPage.TryGetPlayerResponse() ??
                throw VideoUnplayableException.Unplayable(videoId);

            var previewVideoId = playerResponse.TryGetPreviewVideoId();
            if (!string.IsNullOrWhiteSpace(previewVideoId))
                throw VideoRequiresPurchaseException.Preview(videoId, previewVideoId);

            var playerSourceUrl = watchPage.TryGetPlayerSourceUrl() ?? playerConfig?.GetPlayerSourceUrl();
            var playerSource = !string.IsNullOrWhiteSpace(playerSourceUrl)
                ? await PlayerSource.GetAsync(_httpClient, playerSourceUrl)
                : null;

            var cipherOperations = playerSource?.GetCipherOperations().ToArray() ?? Array.Empty<ICipherOperation>();

            if (!playerResponse.IsVideoPlayable())
                throw VideoUnplayableException.Unplayable(videoId, playerResponse.TryGetVideoPlayabilityError());

            if (playerResponse.IsLive())
                throw VideoUnplayableException.LiveStream(videoId);

            var streamInfoProviders = new List<IStreamInfoProvider>();

            // Streams from player config
            if (playerConfig != null)
            {
                streamInfoProviders.AddRange(playerConfig.GetStreams());
            }

            // Streams from player response
            streamInfoProviders.AddRange(playerResponse.GetStreams());

            // Streams from DASH manifest
            var dashManifestUrl = playerResponse.TryGetDashManifestUrl();
            if (!string.IsNullOrWhiteSpace(dashManifestUrl))
            {
                var dashManifest = await GetDashManifestAsync(dashManifestUrl, cipherOperations);
                streamInfoProviders.AddRange(dashManifest.GetStreams());
            }

            return new StreamContext(streamInfoProviders, cipherOperations);
        }

        private async Task<StreamManifest> GetManifestAsync(StreamContext streamContext)
        {
            // To make sure there are no duplicates streams, group them by tag
            var streams = new Dictionary<int, IStreamInfo>();

            foreach (var streamInfo in streamContext.StreamInfoProviders)
            {
                var tag = streamInfo.GetTag();
                var url = streamInfo.GetUrl();

                // Signature
                var signature = streamInfo.TryGetSignature();
                var signatureParameter = streamInfo.TryGetSignatureParameter() ?? "signature";

                if (!string.IsNullOrWhiteSpace(signature))
                {
                    signature = streamContext.CipherOperations.Decipher(signature);
                    url = Url.SetQueryParameter(url, signatureParameter, signature);
                }

                // Content length
                var contentLength =
                    streamInfo.TryGetContentLength() ??
                    await _httpClient.TryGetContentLengthAsync(url, false) ??
                    0;

                if (contentLength <= 0)
                    continue; // broken stream URL?

                // Common
                var container = new Container(streamInfo.GetContainer());
                var fileSize = new FileSize(contentLength);
                var bitrate = new Bitrate(streamInfo.GetBitrate());

                var audioCodec = streamInfo.TryGetAudioCodec();
                var videoCodec = streamInfo.TryGetVideoCodec();

                // Muxed or Video-only
                if (!string.IsNullOrWhiteSpace(videoCodec))
                {
                    var framerate = new Framerate(streamInfo.TryGetFramerate() ?? 24);

                    var videoQualityLabel =
                        streamInfo.TryGetVideoQualityLabel() ??
                        Heuristics.GetVideoQualityLabel(tag, framerate.FramesPerSecond);

                    var videoQuality = Heuristics.GetVideoQuality(videoQualityLabel);

                    var videoWidth = streamInfo.TryGetVideoWidth();
                    var videoHeight = streamInfo.TryGetVideoHeight();
                    var videoResolution = videoWidth != null && videoHeight != null
                        ? new VideoResolution(videoWidth.Value, videoHeight.Value)
                        : Heuristics.GetVideoResolution(videoQuality);

                    // Muxed
                    if (!string.IsNullOrWhiteSpace(audioCodec))
                    {
                        streams[tag] = new MuxedStreamInfo(
                            tag,
                            url,
                            container,
                            fileSize,
                            bitrate,
                            audioCodec,
                            videoCodec,
                            videoQualityLabel,
                            videoQuality,
                            videoResolution,
                            framerate
                        );
                    }
                    // Video-only
                    else
                    {
                        streams[tag] = new VideoOnlyStreamInfo(
                            tag,
                            url,
                            container,
                            fileSize,
                            bitrate,
                            videoCodec,
                            videoQualityLabel,
                            videoQuality,
                            videoResolution,
                            framerate
                        );
                    }
                }
                // Audio-only
                else if (!string.IsNullOrWhiteSpace(audioCodec))
                {
                    streams[tag] = new AudioOnlyStreamInfo(
                        tag,
                        url,
                        container,
                        fileSize,
                        bitrate,
                        audioCodec
                    );
                }
                else
                {
#if DEBUG
                    throw FatalFailureException.Generic("Stream info doesn't contain audio/video codec information.");
#endif
                }
            }

            return new StreamManifest(streams.Values.ToArray());
        }

        /// <summary>
        /// Gets the manifest that contains information about available streams in the specified video.
        /// </summary>
        public async Task<StreamManifest> GetManifestAsync(VideoId videoId)
        {
            // We can try to extract the manifest from two sources: get_video_info and the video watch page.
            // In some cases one works, in some cases another does.

            try
            {
                var context = await GetSteamContextFromVideoInfoAsync(videoId);
                return await GetManifestAsync(context);
            }
            catch (YoutubeExplodeException)
            {
                var context = await GetStreamContextFromWatchPageAsync(videoId);
                return await GetManifestAsync(context);
            }
        }

        /// <summary>
        /// Gets the HTTP Live Stream (HLS) manifest URL for the specified video (if it's a live video stream).
        /// </summary>
        public async Task<string> GetHttpLiveStreamUrlAsync(VideoId videoId)
        {
            var videoInfoResponse = await VideoInfoResponse.GetAsync(_httpClient, videoId);
            var playerResponse = videoInfoResponse.GetPlayerResponse();

            if (!playerResponse.IsVideoPlayable())
                throw VideoUnplayableException.Unplayable(videoId, playerResponse.TryGetVideoPlayabilityError());

            return
                playerResponse.TryGetHlsManifestUrl() ??
                throw VideoUnplayableException.NotLiveStream(videoId);
        }

        /// <summary>
        /// Gets the actual stream which is identified by the specified metadata.
        /// </summary>
        public Task<Stream> GetAsync(IStreamInfo streamInfo)
        {
            // YouTube streams are often rate-limited -- they return data at about the same rate
            // as the actual video is going. This helps them avoid unnecessary bandwidth by not loading
            // all data eagerly. On the other hand, we want to download the streams as fast as possible,
            // so we'll be splitting the stream into small segments and retrieving them separately, to
            // work around rate limiting.

            var segmentSize = streamInfo.IsRateLimited()
                ? 9_898_989 // this number was carefully devised through research
                : (long?) null; // don't use segmentation for non-rate-limited streams

            var stream = new YoutubeMediaStream(
                _httpClient,
                streamInfo.Url,
                streamInfo.Size.TotalBytes,
                segmentSize
            );

            return Task.FromResult<Stream>(stream);
        }

        /// <summary>
        /// Copies the actual stream which is identified by the specified metadata to the specified stream.
        /// </summary>
        public async Task CopyToAsync(IStreamInfo streamInfo, Stream destination,
            IProgress<double>? progress = null, CancellationToken cancellationToken = default)
        {
            using var input = await GetAsync(streamInfo);
            await input.CopyToAsync(destination, progress, cancellationToken);
        }

        /// <summary>
        /// Download the actual stream which is identified by the specified metadata to the specified file.
        /// </summary>
        public async Task DownloadAsync(IStreamInfo streamInfo, string filePath,
            IProgress<double>? progress = null, CancellationToken cancellationToken = default)
        {
            using var destination = File.Create(filePath);
            await CopyToAsync(streamInfo, destination, progress, cancellationToken);
        }
    }

    public partial class StreamClient
    {
        private class StreamContext
        {
            public IReadOnlyList<IStreamInfoProvider> StreamInfoProviders { get; }

            public IReadOnlyList<ICipherOperation> CipherOperations { get; }

            public StreamContext(
                IReadOnlyList<IStreamInfoProvider> streamInfoProviders,
                IReadOnlyList<ICipherOperation> cipherOperations)
            {
                StreamInfoProviders = streamInfoProviders;
                CipherOperations = cipherOperations;
            }
        }
    }
}