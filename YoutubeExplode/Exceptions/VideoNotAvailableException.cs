using System;
using YoutubeExplode.Internal;

namespace YoutubeExplode.Exceptions
{
    /// <summary>
    /// Thrown when video is not available
    /// </summary>
    public class VideoNotAvailableException : Exception
    {
        /// <summary>
        /// ID of the video
        /// </summary>
        public string VideoId { get; }

        /// <summary>
        /// Error code
        /// </summary>
        public int Code { get; }

        /// <summary>
        /// Error reason
        /// </summary>
        public string Reason { get; }

        /// <summary />
        public VideoNotAvailableException(string videoId, int code, string reason)
            : base("The video is not available and cannot be processed")
        {
            VideoId = videoId.GuardNotNull(nameof(videoId));
            Code = code;
            Reason = reason.GuardNotNull(nameof(reason));
        }
    }
}