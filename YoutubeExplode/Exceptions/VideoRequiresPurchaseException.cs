using System;

namespace YoutubeExplode.Exceptions
{
    /// <summary>
    /// Thrown when the video is a paid Youtube Red video since then we have no access to more video details
    /// </summary>
    public class VideoRequiresPurchaseException : Exception
    {
        /// <summary>
        /// Id of the free preview video
        /// </summary>
        public string PreviewVideoId { get; }

        /// <inheritdoc />
        public VideoRequiresPurchaseException(string previewVideoId)
            : base("The video is a paid Youtube Red video and cannot be processed")
        {
            PreviewVideoId = previewVideoId;
        }
    }
}
