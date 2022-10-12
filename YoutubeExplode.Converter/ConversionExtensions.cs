using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.ClosedCaptions;
using YoutubeExplode.Videos.Streams;

namespace YoutubeExplode.Converter;

/// <summary>
/// Extensions for <see cref="VideoClient" /> that provide interface for downloading videos through FFmpeg.
/// </summary>
public static class ConversionExtensions
{
    /// <summary>
    /// Checks whether the container is a known audio-only container.
    /// </summary>
    [Obsolete("Use Container.IsAudioOnly instead."), ExcludeFromCodeCoverage]
    public static bool IsAudioOnly(this Container container) => container.IsAudioOnly;

    /// <summary>
    /// Downloads specified media streams and closed captions and processes them into a single file.
    /// </summary>
    public static async ValueTask DownloadAsync(
        this VideoClient videoClient,
        IReadOnlyList<IStreamInfo> streamInfos,
        IReadOnlyList<ClosedCaptionTrackInfo> closedCaptionTrackInfos,
        ConversionRequest request,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var ffmpeg = new FFmpeg(request.FFmpegCliFilePath);
        var converter = new Converter(videoClient, ffmpeg, request.Preset);

        await converter.ProcessAsync(
            request.OutputFilePath,
            request.Container,
            streamInfos,
            closedCaptionTrackInfos,
            progress,
            cancellationToken
        );
    }

    /// <summary>
    /// Downloads specified media streams and processes them into a single file.
    /// </summary>
    public static async ValueTask DownloadAsync(
        this VideoClient videoClient,
        IReadOnlyList<IStreamInfo> streamInfos,
        ConversionRequest request,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default) =>
        await videoClient.DownloadAsync(
            streamInfos,
            Array.Empty<ClosedCaptionTrackInfo>(),
            request,
            progress,
            cancellationToken
        );

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
        static IEnumerable<IStreamInfo> GetOptimalStreams(StreamManifest streamManifest, Container container)
        {
            if (streamManifest.GetAudioOnlyStreams().Any() && streamManifest.GetVideoOnlyStreams().Any())
            {
                // Include audio stream
                // Priority: transcoding -> bitrate
                yield return streamManifest
                    .GetAudioOnlyStreams()
                    .OrderByDescending(s => s.Container == container)
                    .ThenByDescending(s => s.Bitrate)
                    .First();

                // Include video stream
                if (!container.IsAudioOnly)
                {
                    // Priority: video quality -> transcoding
                    yield return streamManifest
                        .GetVideoOnlyStreams()
                        .OrderByDescending(s => s.VideoQuality)
                        .ThenByDescending(s => s.Container == container)
                        .First();
                }
            }
            // Use single muxed stream if adaptive streams are not available
            else
            {
                // Priority: video quality -> transcoding
                yield return streamManifest
                    .GetMuxedStreams()
                    .OrderByDescending(s => s.VideoQuality)
                    .ThenByDescending(s => s.Container == container)
                    .First();
            }
        }

        var streamManifest = await videoClient.Streams.GetManifestAsync(videoId, cancellationToken);
        var streamInfos = GetOptimalStreams(streamManifest, request.Container).ToArray();

        await videoClient.DownloadAsync(streamInfos, request, progress, cancellationToken);
    }

    /// <summary>
    /// Resolves the best media streams for the specified video, downloads them and processes into a single file.
    /// </summary>
    /// <remarks>
    /// Output container is derived from file extension, unless explicitly specified.
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

        await videoClient.DownloadAsync(videoId, request, progress, cancellationToken);
    }

    /// <summary>
    /// Resolves the best media streams for the specified video, downloads them and processes into a single file.
    /// </summary>
    /// <remarks>
    /// Output container is derived from file extension.
    /// If none specified, mp4 is chosen by default.
    /// </remarks>
    public static async ValueTask DownloadAsync(
        this VideoClient videoClient,
        VideoId videoId,
        string outputFilePath,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default) =>
        await videoClient.DownloadAsync(videoId, outputFilePath, _ => { }, progress, cancellationToken);
}