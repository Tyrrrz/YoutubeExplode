using System.Diagnostics.CodeAnalysis;

namespace YoutubeExplode.Videos.Streams;

/// <summary>
/// Metadata associated with an audio-only YouTube media stream.
/// </summary>
public class AudioOnlyStreamInfo(
    string url,
    Container container,
    FileSize size,
    Bitrate bitrate,
    string audioCodec
) : IAudioStreamInfo
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
    [ExcludeFromCodeCoverage]
    public override string ToString() => $"Audio-only ({Container})";
}
