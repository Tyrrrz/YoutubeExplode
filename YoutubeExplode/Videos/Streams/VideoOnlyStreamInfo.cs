using System.Diagnostics.CodeAnalysis;
using YoutubeExplode.Common;

namespace YoutubeExplode.Videos.Streams;

/// <summary>
/// Metadata associated with a video-only media stream.
/// </summary>
public class VideoOnlyStreamInfo(
    string url,
    Container container,
    FileSize size,
    Bitrate bitrate,
    string videoCodec,
    VideoQuality videoQuality,
    Resolution videoResolution
) : IVideoStreamInfo
{
    /// <inheritdoc />
    public string Url { get; } = url;

    /// <inheritdoc />
    public Container Container { get; } = container;

    /// <inheritdoc />
    public FileSize Size { get; } = size;

    /// <inheritdoc />
    public Bitrate Bitrate { get; } = bitrate;

    /// <inheritdoc />
    public string VideoCodec { get; } = videoCodec;

    /// <inheritdoc />
    public VideoQuality VideoQuality { get; } = videoQuality;

    /// <inheritdoc />
    public Resolution VideoResolution { get; } = videoResolution;

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public override string ToString() => $"Video-only ({VideoQuality} | {Container})";
}
