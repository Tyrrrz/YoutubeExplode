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
using YoutubeExplode.Bridge.Signature;
using YoutubeExplode.Common;
using YoutubeExplode.Exceptions;
using YoutubeExplode.Utils;
using YoutubeExplode.Utils.Extensions;

namespace YoutubeExplode.Videos.Streams
{
    /// <summary>
    /// Queries related to media streams of YouTube videos.
    /// </summary>
    public class StreamClient
    {
        private readonly HttpClient _httpClient;
        private readonly YoutubeController _youtubeController;

        /// <summary>
        /// Initializes an instance of <see cref="StreamClient"/>.
        /// </summary>
        public StreamClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _youtubeController = new YoutubeController(httpClient);
        }

        private async ValueTask PopulateStreamInfosAsync(
            ICollection<IStreamInfo> streamInfos,
            IEnumerable<IStreamInfoExtractor> streamInfoExtractors,
            Scrambler scrambler,
            CancellationToken cancellationToken = default)
        {
            foreach (var streamInfoExtractor in streamInfoExtractors)
            {
                var itag =
                    streamInfoExtractor.TryGetItag() ??
                    throw new YoutubeExplodeException("Could not extract stream itag.");

                var url =
                    streamInfoExtractor.TryGetUrl() ??
                    throw new YoutubeExplodeException("Could not extract stream URL.");

                // Unscramble and apply signature
                var signature = streamInfoExtractor.TryGetSignature();
                if (!string.IsNullOrWhiteSpace(signature))
                {
                    var signatureParameter = streamInfoExtractor.TryGetSignatureParameter() ?? "signature";
                    var unscrambledSignature = scrambler.Unscramble(signature);

                    url = Url.SetQueryParameter(url, signatureParameter, unscrambledSignature);
                }

                // Content length
                var contentLength =
                    streamInfoExtractor.TryGetContentLength() ??
                    await _httpClient.TryGetContentLengthAsync(url, false, cancellationToken) ??
                    0;

                if (contentLength <= 0)
                    continue; // broken stream URL?

                // Common metadata
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

                    var videoQuality =
                        streamInfoExtractor.TryGetVideoQualityLabel()?.Pipe(s => VideoQuality.FromLabel(s, framerate)) ??
                        VideoQuality.FromItag(itag, framerate);

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
            var watchPage = await _youtubeController.GetVideoWatchPageAsync(videoId, cancellationToken);

            // Try to get player source (failing is ok because there's a decent chance we won't need it)
            var playerSourceUrl = watchPage.TryGetPlayerSourceUrl();
            var playerSource = !string.IsNullOrWhiteSpace(playerSourceUrl)
                ? await _youtubeController.GetPlayerSourceAsync(playerSourceUrl, cancellationToken)
                : null;

            var scrambler = playerSource?.TryGetScrambler() ?? Scrambler.Null;

            var playerResponseFromWatchPage = watchPage.TryGetPlayerResponse();
            if (playerResponseFromWatchPage is not null)
            {
                // todo: check

                // Extract streams from watch page
                await PopulateStreamInfosAsync(
                    streamInfos,
                    watchPage.GetStreams(),
                    scrambler,
                    cancellationToken
                );

                // Extract streams from player response
                await PopulateStreamInfosAsync(
                    streamInfos,
                    playerResponseFromWatchPage.GetStreams(),
                    scrambler,
                    cancellationToken
                );

                // Extract stream from DASH manifest
                var dashManifestUrl = playerResponseFromWatchPage.TryGetDashManifestUrl();
                if (!string.IsNullOrWhiteSpace(dashManifestUrl))
                {
                    var signature = Regex.Match(dashManifestUrl, "/s/(.*?)(?:/|$)").Groups[1].Value;

                    if (!string.IsNullOrWhiteSpace(signature))
                    {
                        var unscrambledSignature = scrambler.Unscramble(signature);
                        dashManifestUrl = Url.SetRouteParameter(dashManifestUrl, "signature", unscrambledSignature);
                    }

                    var dashManifest = await _youtubeController.GetDashManifestAsync(dashManifestUrl, cancellationToken);

                    await PopulateStreamInfosAsync(
                        streamInfos,
                        dashManifest.GetStreams(),
                        scrambler,
                        cancellationToken
                    );
                }

                // If successfully retrieved streams, return
                if (streamInfos.Any())
                {
                    return;
                }
            }

            // Try to get streams from video info
            var signatureTimestamp = playerSource?.TryGetSignatureTimestamp() ?? "";
            var videoInfo = await _youtubeController.GetVideoInfoAsync(videoId, signatureTimestamp, cancellationToken);

            var playerResponseFromVideoInfo = videoInfo.TryGetPlayerResponse();
            if (playerResponseFromVideoInfo is not null)
            {
                // todo: check

                // Extract streams from video info
                await PopulateStreamInfosAsync(
                    streamInfos,
                    videoInfo.GetStreams(),
                    scrambler,
                    cancellationToken
                );

                // Extract streams from player response
                await PopulateStreamInfosAsync(
                    streamInfos,
                    playerResponseFromVideoInfo.GetStreams(),
                    scrambler,
                    cancellationToken
                );

                // Extract stream from DASH manifest
                var dashManifestUrl = playerResponseFromVideoInfo.TryGetDashManifestUrl();
                if (!string.IsNullOrWhiteSpace(dashManifestUrl))
                {
                    var signature = Regex.Match(dashManifestUrl, "/s/(.*?)(?:/|$)").Groups[1].Value;

                    if (!string.IsNullOrWhiteSpace(signature))
                    {
                        var unscrambledSignature = scrambler.Unscramble(signature);
                        dashManifestUrl = Url.SetRouteParameter(dashManifestUrl, "signature", unscrambledSignature);
                    }

                    var dashManifest = await _youtubeController.GetDashManifestAsync(dashManifestUrl, cancellationToken);

                    await PopulateStreamInfosAsync(
                        streamInfos,
                        dashManifest.GetStreams(),
                        scrambler,
                        cancellationToken
                    );
                }
            }
        }

        /// <summary>
        /// Gets the manifest that contains information about available streams in the specified video.
        /// </summary>
        public async ValueTask<StreamManifest> GetManifestAsync(
            VideoId videoId,
            CancellationToken cancellationToken = default)
        {
            var streamInfos = new List<IStreamInfo>();
            await PopulateStreamInfosAsync(streamInfos, videoId, cancellationToken);

            if (!streamInfos.Any())
            {
                throw VideoUnplayableException.Unplayable(videoId);
            }

            return new StreamManifest(streamInfos);
        }

        /// <summary>
        /// Gets the HTTP Live Stream (HLS) manifest URL for the specified video (if it is a livestream).
        /// </summary>
        public async ValueTask<string> GetHttpLiveStreamUrlAsync(
            VideoId videoId,
            CancellationToken cancellationToken = default)
        {
            var videoInfo = await _youtubeController.GetVideoInfoAsync(videoId, cancellationToken);

            var playerResponse =
                videoInfo.TryGetPlayerResponse() ??
                throw new YoutubeExplodeException("Could not extract player response.");

            if (!playerResponse.IsVideoPlayable())
                throw VideoUnplayableException.Unplayable(videoId, playerResponse.TryGetVideoPlayabilityError());

            var hlsUrl = playerResponse.TryGetHlsManifestUrl();
            if (string.IsNullOrWhiteSpace(hlsUrl))
            {
                throw new YoutubeExplodeException(
                    "Could not extract HTTP Live Stream manifest URL. " +
                    "The video is likely not a live stream."
                );
            }

            return hlsUrl;
        }

        /// <summary>
        /// Gets the actual stream which is identified by the specified metadata.
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
                streamInfo.Size.TotalBytes,
                segmentSize
            );

            // Pre-resolve inner stream eagerly
            await stream.PreloadAsync(cancellationToken);

            return stream;
        }

        /// <summary>
        /// Copies the actual stream which is identified by the specified metadata to the destination stream.
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
        /// Download the actual stream which is identified by the specified metadata to the destination file.
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