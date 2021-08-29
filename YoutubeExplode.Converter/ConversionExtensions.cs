using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using YoutubeExplode.Converter.Utils;
using YoutubeExplode.Converter.Utils.Extensions;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace YoutubeExplode.Converter
{
    /// <summary>
    /// Extensions for <see cref="VideoClient"/> that provide interface for downloading videos through FFmpeg.
    /// </summary>
    public static class ConversionExtensions
    {
        private static bool IsTranscodingRequired(Container container, ConversionFormat format) =>
            !string.Equals(container.Name, format.Name, StringComparison.OrdinalIgnoreCase);

        private static IEnumerable<IStreamInfo> GetBestMediaStreamInfos(
            StreamManifest streamManifest,
            ConversionFormat format)
        {
            // Use single muxed stream if adaptive streams are not available
            if (!streamManifest.GetAudioOnlyStreams().Any() || !streamManifest.GetVideoOnlyStreams().Any())
            {
                // Priority: video quality -> transcoding
                yield return streamManifest
                    .GetMuxedStreams()
                    .OrderByDescending(s => s.VideoQuality)
                    .ThenByDescending(s => !IsTranscodingRequired(s.Container, format))
                    .First();

                yield break;
            }

            // Include audio stream
            // Priority: transcoding -> bitrate
            yield return streamManifest
                .GetAudioOnlyStreams()
                .OrderByDescending(s => !IsTranscodingRequired(s.Container, format))
                .ThenByDescending(s => s.Bitrate)
                .First();

            // Include video stream
            if (!format.IsAudioOnly)
            {
                // Priority: video quality -> transcoding
                yield return streamManifest
                    .GetVideoOnlyStreams()
                    .OrderByDescending(s => s.VideoQuality)
                    .ThenByDescending(s => !IsTranscodingRequired(s.Container, format))
                    .First();
            }
        }

        /// <summary>
        /// Downloads specified media streams and processes them into a single file.
        /// </summary>
        public static async ValueTask DownloadAsync(
            this VideoClient videoClient,
            IReadOnlyList<IStreamInfo> streamInfos,
            ConversionRequest request,
            IProgress<double>? progress = null,
            CancellationToken cancellationToken = default)
        {
            Platform.EnsureDesktop();

            // Ensure that the provided stream collection is not empty
            if (!streamInfos.Any())
                throw new InvalidOperationException("No streams provided.");

            // If all streams have the same container as the output format, then transcoding is not required
            var isTranscodingRequired = streamInfos.Any(s => IsTranscodingRequired(s.Container, request.Format));

            // Progress setup
            var progressMixer = progress?.Pipe(p => new ProgressMixer(p));
            var downloadProgressPortion = isTranscodingRequired ? 0.15 : 0.99;
            var totalStreamSize = streamInfos.Sum(s => s.Size.Bytes);

            // Temp files for streams
            var streamFilePaths = new List<string>(streamInfos.Count);

            try
            {
                // Download streams
                foreach (var streamInfo in streamInfos)
                {
                    var streamIndex = streamFilePaths.Count + 1;
                    var streamFilePath = $"{request.OutputFilePath}.stream-{streamIndex}.tmp";

                    streamFilePaths.Add(streamFilePath);

                    var streamDownloadProgress = progressMixer?.Split(
                        downloadProgressPortion * streamInfo.Size.Bytes / totalStreamSize
                    );

                    await videoClient.Streams.DownloadAsync(
                        streamInfo,
                        streamFilePath,
                        streamDownloadProgress,
                        cancellationToken
                    ).ConfigureAwait(false);
                }

                // Mux/convert streams
                var conversionProgress = progressMixer?.Split(1 - downloadProgressPortion);

                await new FFmpeg(request.FFmpegCliFilePath).ExecuteAsync(
                    streamFilePaths,
                    request.OutputFilePath,
                    request.Format.Name,
                    request.Preset.ToString().ToLowerInvariant(),
                    isTranscodingRequired,
                    conversionProgress,
                    cancellationToken
                ).ConfigureAwait(false);

                progress?.Report(1);
            }
            finally
            {
                // Delete temp files
                foreach (var streamFilePath in streamFilePaths)
                {
                    try
                    {
                        File.Delete(streamFilePath);
                    }
                    catch
                    {
                        // Try our best but don't crash
                    }
                }
            }
        }

        /// <summary>
        /// Resolves the best media streams for the specified video, downloads them and processes into a single file.
        /// </summary>
        public static async ValueTask DownloadAsync(
            this VideoClient videoClient,
            VideoId videoId,
            ConversionRequest request,
            IProgress<double>? progress = null,
            CancellationToken cancellationToken = default)
        {
            var streamManifest = await videoClient.Streams.GetManifestAsync(videoId, cancellationToken)
                .ConfigureAwait(false);

            var streamInfos = GetBestMediaStreamInfos(streamManifest, request.Format).ToArray();

            await videoClient.DownloadAsync(
                streamInfos,
                request,
                progress,
                cancellationToken
            ).ConfigureAwait(false);
        }

        /// <summary>
        /// Resolves the best media streams for the specified video, downloads them and processes into a single file.
        /// </summary>
        /// <remarks>
        /// Conversion format is derived from file extension, unless explicitly specified.
        /// </remarks>
        public static async ValueTask DownloadAsync(
            this VideoClient videoClient,
            VideoId videoId,
            string outputFilePath,
            Action<ConversionRequestBuilder> configure,
            IProgress<double>? progress = null,
            CancellationToken cancellationToken = default)
        {
            var requestBuilder = new ConversionRequestBuilder(outputFilePath);
            configure(requestBuilder);
            var request = requestBuilder.Build();

            await videoClient.DownloadAsync(videoId, request, progress, cancellationToken)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Resolves the best media streams for the specified video, downloads them and processes into a single file.
        /// </summary>
        /// <remarks>
        /// Conversion format is derived from file extension.
        /// If none specified, mp4 is chosen by default.
        /// </remarks>
        public static async ValueTask DownloadAsync(
            this VideoClient videoClient,
            VideoId videoId,
            string outputFilePath,
            IProgress<double>? progress = null,
            CancellationToken cancellationToken = default)
        {
            await videoClient.DownloadAsync(videoId, outputFilePath, _ => { }, progress, cancellationToken)
                .ConfigureAwait(false);
        }
    }
}