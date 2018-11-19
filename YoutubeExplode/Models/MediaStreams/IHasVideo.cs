using JetBrains.Annotations;

namespace YoutubeExplode.Models.MediaStreams
{
    /// <summary>
    /// Implemented by <see cref="MediaStreamInfo"/> that contain video.
    /// </summary>
    public interface IHasVideo
    {
        /// <summary>
        /// Video encoding of the associated stream.
        /// </summary>
        [NotNull]
        string VideoEncoding { get; }

        /// <summary>
        /// Video quality label of the associated stream.
        /// </summary>
        [NotNull]
        string VideoQualityLabel { get; }

        /// <summary>
        /// Video quality of the associated stream.
        /// </summary>
        VideoQuality VideoQuality { get; }

        /// <summary>
        /// Video resolution of the associated stream.
        /// </summary>
        VideoResolution Resolution { get; }

        /// <summary>
        /// Video framerate (FPS) of the associated stream.
        /// </summary>
        int Framerate { get; }
    }
}