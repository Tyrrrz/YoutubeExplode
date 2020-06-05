namespace YoutubeExplode.Videos.Streams
{
    /// <summary>
    /// YouTube media stream that contains audio.
    /// </summary>
    public interface IAudioStreamInfo : IStreamInfo
    {
        /// <summary>
        /// Audio codec.
        /// </summary>
        string AudioCodec { get; }
    }
}