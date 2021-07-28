using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using YoutubeExplode.Bridge;
using YoutubeExplode.Bridge.Extractors;
using YoutubeExplode.Bridge.SignatureScrambling;
using YoutubeExplode.Common;
using YoutubeExplode.Exceptions;
using YoutubeExplode.Utils;
using YoutubeExplode.Utils.Extensions;

namespace YoutubeExplode.Videos.Streams
{
    /// <summary>
    /// Operations related to media streams of YouTube videos.
    /// </summary>
    public class StreamClient
    {
        private readonly HttpClient _httpClient;
        private readonly YoutubeController _controller;

        /// <summary>
        /// Initializes an instance of <see cref="StreamClient"/>.
        /// </summary>
        public StreamClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _controller = new YoutubeController(httpClient);
        }

        private string UnscrambleStreamUrl(
            SignatureScrambler signatureScrambler,
            string streamUrl,
            string? signature,
            string? signatureParameter)
        {
            if (string.IsNullOrWhiteSpace(signature))
                return streamUrl;

            return Url.SetQueryParameter(
                streamUrl,
                signatureParameter ?? "signature",
                signatureScrambler.Unscramble(signature)
            );
        }

        private string UnscrambleDashManifestUrl(
            SignatureScrambler signatureScrambler,
            string dashManifestUrl)
        {
            var signature = Regex.Match(dashManifestUrl, "/s/(.*?)(?:/|$)").Groups[1].Value;
            if (string.IsNullOrWhiteSpace(signature))
                return dashManifestUrl;

            return Url.SetRouteParameter(
                dashManifestUrl,
                "signature",
                signatureScrambler.Unscramble(signature)
            );
        }

        private async ValueTask PopulateStreamInfosAsync(
            ICollection<IStreamInfo> streamInfos,
            IEnumerable<IStreamInfoExtractor> streamInfoExtractors,
            SignatureScrambler signatureScrambler,
            CancellationToken cancellationToken = default)
        {
            foreach (var streamInfoExtractor in streamInfoExtractors)
            {
                var itag =
                    streamInfoExtractor.TryGetItag() ??
                    throw new YoutubeExplodeException("Could not extract stream itag.");

                var urlRaw =
                    streamInfoExtractor.TryGetUrl() ??
                    throw new YoutubeExplodeException("Could not extract stream URL.");

                // Unscramble URL
                var signature = streamInfoExtractor.TryGetSignature();
                var signatureParameter = streamInfoExtractor.TryGetSignatureParameter();
                var url = UnscrambleStreamUrl(signatureScrambler, urlRaw, signature, signatureParameter);

                // Get content length
                var contentLength =
                    streamInfoExtractor.TryGetContentLength() ??
                    await _httpClient.TryGetContentLengthAsync(url, false, cancellationToken) ??
                    0;

                if (contentLength <= 0)
                    continue; // broken stream URL?

                var fileSize = new FileSize(contentLength);

                var container =
                    streamInfoExtractor.TryGetContainer()?.Pipe(s => new Container(s)) ??
                    throw new YoutubeExplodeException("Could not extract stream container.");

                var bitrate =
                    streamInfoExtractor.TryGetBitrate()?.Pipe(s => new Bitrate(s)) ??
                    throw new YoutubeExplodeException("Could not extract stream bitrate.");

                var audioCodec = streamInfoExtractor.TryGetAudioCodec();
                var videoCodec = streamInfoExtractor.TryGetVideoCodec();

                // Muxed or video-only stream
                if (!string.IsNullOrWhiteSpace(videoCodec))
                {
                    var framerate = streamInfoExtractor.TryGetFramerate() ?? 24;

                    var videoQualityLabel = streamInfoExtractor.TryGetVideoQualityLabel();

                    var videoQuality = !string.IsNullOrWhiteSpace(videoQualityLabel)
                        ? VideoQuality.FromLabel(videoQualityLabel, framerate)
                        : VideoQuality.FromItag(itag, framerate);

                    var videoWidth = streamInfoExtractor.TryGetVideoWidth();
                    var videoHeight = streamInfoExtractor.TryGetVideoHeight();

                    var videoResolution = videoWidth is not null && videoHeight is not null
                        ? new Resolution(videoWidth.Value, videoHeight.Value)
                        : videoQuality.GetDefaultVideoResolution();

                    // Muxed
                    if (!string.IsNullOrWhiteSpace(audioCodec))
                    {
                        var streamInfo = new MuxedStreamInfo(
                            url,
                            container,
                            fileSize,
                            bitrate,
                            audioCodec,
                            videoCodec,
                            videoQuality,
                            videoResolution
                        );

                        streamInfos.Add(streamInfo);
                    }
                    // Video-only
                    else
                    {
                        var streamInfo = new VideoOnlyStreamInfo(
                            url,
                            container,
                            fileSize,
                            bitrate,
                            videoCodec,
                            videoQuality,
                            videoResolution
                        );

                        streamInfos.Add(streamInfo);
                    }
                }
                // Audio-only
                else if (!string.IsNullOrWhiteSpace(audioCodec))
                {
                    var streamInfo = new AudioOnlyStreamInfo(
                        url,
                        container,
                        fileSize,
                        bitrate,
                        audioCodec
                    );

                    streamInfos.Add(streamInfo);
                }
                else
                {
                    Debug.Fail("Stream doesn't contain neither audio nor video codec information.");
                }
            }
        }

        private async ValueTask PopulateStreamInfosAsync(
            ICollection<IStreamInfo> streamInfos,
            VideoId videoId,
            CancellationToken cancellationToken = default)
        {
            var watchPage = await _controller.GetVideoWatchPageAsync(videoId, cancellationToken);

            // Try to get player source (failing is ok because there's a decent chance we won't need it)
            var playerSourceUrl = watchPage.TryGetPlayerSourceUrl();
            var playerSource = !string.IsNullOrWhiteSpace(playerSourceUrl)
                ? await _controller.GetPlayerSourceAsync(playerSourceUrl, cancellationToken)
                : null;

            var signatureScrambler = playerSource?.TryGetSignatureScrambler() ?? SignatureScrambler.Null;

            var playerResponseFromWatchPage = watchPage.TryGetPlayerResponse();
            if (playerResponseFromWatchPage is not null)
            {
                var purchasePreviewVideoId = playerResponseFromWatchPage.TryGetPreviewVideoId();
                if (!string.IsNullOrWhiteSpace(purchasePreviewVideoId))
                {
                    throw new VideoRequiresPurchaseException(
                        $"Video '{videoId}' requires purchase and cannot be played.",
                        purchasePreviewVideoId
                    );
                }

                if (playerResponseFromWatchPage.IsVideoPlayable())
                {
                    // Extract streams from watch page
                    await PopulateStreamInfosAsync(
                        streamInfos,
                        watchPage.GetStreams(),
                        signatureScrambler,
                        cancellationToken
                    );

                    // Extract streams from player response
                    await PopulateStreamInfosAsync(
                        streamInfos,
                        playerResponseFromWatchPage.GetStreams(),
                        signatureScrambler,
                        cancellationToken
                    );

                    // Extract streams from DASH manifest
                    var dashManifestUrlRaw = playerResponseFromWatchPage.TryGetDashManifestUrl();
                    if (!string.IsNullOrWhiteSpace(dashManifestUrlRaw))
                    {
                        var dashManifestUrl = UnscrambleDashManifestUrl(signatureScrambler, dashManifestUrlRaw);
                        var dashManifest = await _controller.GetDashManifestAsync(dashManifestUrl, cancellationToken);

                        await PopulateStreamInfosAsync(
                            streamInfos,
                            dashManifest.GetStreams(),
                            signatureScrambler,
                            cancellationToken
                        );
                    }
                }

                // If successfully retrieved streams, return
                if (streamInfos.Any())
                {
                    return;
                }
            }

            // Couldn't extract any streams
            throw new VideoUnplayableException($"Video '{videoId}' does not contain any playable streams.");
        }

        /// <summary>
        /// Gets the manifest containing information about available streams on the specified video.
        /// </summary>
        public async ValueTask<StreamManifest> GetManifestAsync(
            VideoId videoId,
            CancellationToken cancellationToken = default)
        {
            var streamInfos = new List<IStreamInfo>();
            await PopulateStreamInfosAsync(streamInfos, videoId, cancellationToken);

            return new StreamManifest(streamInfos);
        }

        /// <summary>
        /// Gets the HTTP Live Stream (HLS) manifest URL for the specified video (if it is a livestream).
        /// </summary>
        public async ValueTask<string> GetHttpLiveStreamUrlAsync(
            VideoId videoId,
            CancellationToken cancellationToken = default)
        {
            var watchPage = await _controller.GetVideoWatchPageAsync(videoId, cancellationToken);

            var playerResponse =
                watchPage.TryGetPlayerResponse() ??
                throw new YoutubeExplodeException("Could not extract player response.");

            if (!playerResponse.IsVideoPlayable())
            {
                var errorMessage = playerResponse.TryGetVideoPlayabilityError();
                throw new VideoUnplayableException($"Video '{videoId}' is unplayable. Reason: {errorMessage}.");
            }

            var hlsUrl = playerResponse.TryGetHlsManifestUrl();
            if (string.IsNullOrWhiteSpace(hlsUrl))
            {
                throw new YoutubeExplodeException(
                    "Could not extract HTTP Live Stream manifest URL. " +
                    $"Video '{videoId}' is likely not a live stream."
                );
            }

            return hlsUrl;
        }

        /// <summary>
        /// Gets the stream identified by the specified metadata.
        /// </summary>
        public async ValueTask<Stream> GetAsync(
            IStreamInfo streamInfo,
            CancellationToken cancellationToken = default)
        {
            // For most streams, YouTube limits transfer speed to match the video playback rate.
            // This helps them avoid unnecessary bandwidth, but for us it's a hindrance because
            // we want to download the stream as fast as possible.
            // To solve this, we divide the logical stream up into multiple segments and download
            // them all separately.

            var isThrottled = !Regex.IsMatch(streamInfo.Url, "ratebypass[=/]yes");

            var segmentSize = isThrottled
                ? 9_898_989 // breakpoint after which the throttling kicks in
                : (long?) null; // no segmentation for non-throttled streams

            var stream = new SegmentedHttpStream(
                _httpClient,
                streamInfo.Url,
                streamInfo.Size.Bytes,
                segmentSize
            );

            // Pre-resolve inner stream eagerly
            await stream.PreloadAsync(cancellationToken);

            return stream;
        }

        /// <summary>
        /// Copies the stream identified by the specified metadata to the specified stream.
        /// </summary>
        public async ValueTask CopyToAsync(
            IStreamInfo streamInfo,
            Stream destination,
            IProgress<double>? progress = null,
            CancellationToken cancellationToken = default)
        {
            using var input = await GetAsync(streamInfo, cancellationToken);
            await input.CopyToAsync(destination, progress, cancellationToken);
        }

        /// <summary>
        /// Downloads the stream identified by the specified metadata to the specified file.
        /// </summary>
        public async ValueTask DownloadAsync(
            IStreamInfo streamInfo,
            string filePath,
            IProgress<double>? progress = null,
            CancellationToken cancellationToken = default)
        {
            using var destination = File.Create(filePath);
            await CopyToAsync(streamInfo, destination, progress, cancellationToken);
        }
    }
}