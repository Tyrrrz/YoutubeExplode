using System;

namespace YoutubeExplode.Exceptions
{
    /// <summary>
    /// Thrown when the video is a paid Youtube Red video since then we have no access to more video details
    /// </summary>
    public class VideoRequiresPurchaseException : Exception
    {
        /// <inheritdoc />
        public VideoRequiresPurchaseException()
            : base()
        {
        }
    }
}
