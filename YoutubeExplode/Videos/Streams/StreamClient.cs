using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using YoutubeExplode.Exceptions;
using YoutubeExplode.Extraction;
using YoutubeExplode.Extraction.Responses;
using YoutubeExplode.Extraction.Responses.Signature;
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

        private async ValueTask<DashManifest> GetDashManifestAsync(
            string dashManifestUrl,
            IReadOnlyList<IScramblerOperation> cipherOperations)
        {
            var signature = DashManifest.TryGetSignatureFromUrl(dashManifestUrl);

            if (!string.IsNullOrWhiteSpace(signature))
            {
                signature = cipherOperations.Decipher(signature);
                dashManifestUrl = Url.SetRouteParameter(dashManifestUrl, "signature", signature);
            }

            return await DashManifest.GetAsync(_httpClient, dashManifestUrl);
        }

        /// <summary>
        /// Gets the manifest that contains information about available streams in the specified video.
        /// </summary>
        public async ValueTask<StreamManifest> GetManifestAsync(
            VideoId videoId,
            CancellationToken cancellationToken = default)
        {
            var watchPage = await _youtubeController.GetVideoWatchPageAsync(videoId, cancellationToken);
        }

        /// <summary>
        /// Gets the HTTP Live Stream (HLS) manifest URL for the specified video (if it's a live video stream).
        /// </summary>
        public async ValueTask<string> GetHttpLiveStreamUrlAsync(
            VideoId videoId,
            CancellationToken cancellationToken = default)
        {
            var videoInfoResponse = await _youtubeController.GetVideoInfoResponseAsync(videoId, cancellationToken);

            var playerResponse =
                videoInfoResponse.TryGetPlayerResponse() ??
                throw new YoutubeExplodeException("Could not extract player response.");

            if (!playerResponse.IsVideoPlayable())
                throw VideoUnplayableException.Unplayable(videoId, playerResponse.TryGetVideoPlayabilityError());

            return
                playerResponse.TryGetHlsManifestUrl() ??
                throw new YoutubeExplodeException("Could not extract HLS URL (the video is likely not a live stream).");
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

            var segmentSize = streamInfo.IsThrottled()
                ? 9_898_989 // breakpoint after which the throttling kicks in
                : (long?) null; // no segmentation for non-throttled streams

            var stream = new SegmentedHttpStream(
                _httpClient,
                streamInfo.Url,
                streamInfo.Size.TotalBytes,
                segmentSize
            );

            await stream.PrepareAsync(cancellationToken);

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