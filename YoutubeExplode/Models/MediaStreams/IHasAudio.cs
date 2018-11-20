using JetBrains.Annotations;

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
        [NotNull]
        string AudioEncoding { get; }
    }
}