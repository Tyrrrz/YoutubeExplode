namespace YoutubeExplode.Models.MediaStreams
{
    /// <summary>
    /// Implemented by <see cref="MediaStreamInfo"/>s that contain audio.
    /// </summary>
    public interface IHasAudio
    {
        /// <summary>
        /// Audio encoding of the associated stream.
        /// </summary>
        AudioEncoding AudioEncoding { get; }
    }
}