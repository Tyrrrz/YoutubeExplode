using System.Diagnostics.CodeAnalysis;
using YoutubeExplode.Common;

namespace YoutubeExplode.Videos.Streams;

/// <summary>
/// Metadata associated with a video-only media stream.
/// </summary>
public class VideoOnlyStreamInfo : IVideoStreamInfo
{
    /// <inheritdoc />
    public string Url { get; }

    /// <inheritdoc />
    public Container Container { get; }

    /// <inheritdoc />
    public FileSize Size { get; }

    /// <inheritdoc />
    public Bitrate Bitrate { get; }

    /// <inheritdoc />
    public string VideoCodec { get; }

    /// <inheritdoc />
    public VideoQuality VideoQuality { get; }

    /// <inheritdoc />
    public Resolution VideoResolution { get; }

    /// <summary>
    /// Initializes an instance of <see cref="VideoOnlyStreamInfo" />.
    /// </summary>
    public VideoOnlyStreamInfo(
        string url,
        Container container,
        FileSize size,
        Bitrate bitrate,
        string videoCodec,
        VideoQuality videoQuality,
        Resolution videoResolution)
    {
        Url = url;
        Container = container;
        Size = size;
        Bitrate = bitrate;
        VideoCodec = videoCodec;
        VideoQuality = videoQuality;
        VideoResolution = videoResolution;
    }

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public override string ToString() => $"Video-only ({VideoQuality} | {Container})";
}