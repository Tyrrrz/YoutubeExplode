using System.Diagnostics.CodeAnalysis;
using YoutubeExplode.Common;
using YoutubeExplode.Videos.ClosedCaptions;

namespace YoutubeExplode.Videos.Streams;

/// <summary>
/// Metadata associated with a muxed (audio + video combined) media stream.
/// </summary>
public class MuxedStreamInfo(
    string url,
    Container container,
    FileSize size,
    Bitrate bitrate,
    string audioCodec,
    Language? audioLanguage,
    bool? isAudioLanguageDefault,
    string videoCodec,
    VideoQuality videoQuality,
    Resolution videoResolution
) : IAudioStreamInfo, IVideoStreamInfo
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
    public string AudioCodec { get; } = audioCodec;

    /// <inheritdoc />
    public Language? AudioLanguage { get; } = audioLanguage;

    /// <inheritdoc />
    public bool? IsAudioLanguageDefault { get; } = isAudioLanguageDefault;

    /// <inheritdoc />
    public string VideoCodec { get; } = videoCodec;

    /// <inheritdoc />
    public VideoQuality VideoQuality { get; } = videoQuality;

    /// <inheritdoc />
    public Resolution VideoResolution { get; } = videoResolution;

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public override string ToString() => $"Muxed ({VideoQuality} | {Container})";
}
