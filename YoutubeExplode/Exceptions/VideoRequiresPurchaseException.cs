using System;
using YoutubeExplode.Internal;

namespace YoutubeExplode.Exceptions
{
    /// <summary>
    /// Thrown when the video requires purchase
    /// </summary>
    public class VideoRequiresPurchaseException : Exception
    {
        /// <summary>
        /// ID of the video
        /// </summary>
        public string VideoId { get; }

        /// <summary>
        /// ID of the free preview video
        /// </summary>
        public string PreviewVideoId { get; }

        /// <summary />
        public VideoRequiresPurchaseException(string videoId, string previewVideoId)
            : base("The video is a paid Youtube Red video and cannot be processed")
        {
            VideoId = videoId.GuardNotNull(nameof(videoId));
            PreviewVideoId = previewVideoId.GuardNotNull(nameof(previewVideoId));
        }
    }
}