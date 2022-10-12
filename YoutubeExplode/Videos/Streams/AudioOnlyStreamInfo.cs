using System.Diagnostics.CodeAnalysis;

namespace YoutubeExplode.Videos.Streams;

/// <summary>
/// Metadata associated with an audio-only YouTube media stream.
/// </summary>
public class AudioOnlyStreamInfo : IAudioStreamInfo
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
    public string AudioCodec { get; }

    /// <summary>
    /// Initializes an instance of <see cref="AudioOnlyStreamInfo" />.
    /// </summary>
    public AudioOnlyStreamInfo(
        string url,
        Container container,
        FileSize size,
        Bitrate bitrate,
        string audioCodec)
    {
        Url = url;
        Container = container;
        Size = size;
        Bitrate = bitrate;
        AudioCodec = audioCodec;
    }

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public override string ToString() => $"Audio-only ({Container})";
}