namespace YoutubeExplode.Videos.Streams;

/// <summary>
/// Metadata associated with a media stream that contains audio.
/// </summary>
public interface IAudioStreamInfo : IStreamInfo
{
    /// <summary>
    /// Audio codec.
    /// </summary>
    string AudioCodec { get; }
}