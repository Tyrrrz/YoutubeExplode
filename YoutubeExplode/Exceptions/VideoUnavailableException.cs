using System;
using YoutubeExplode.Internal;

namespace YoutubeExplode.Exceptions
{
    /// <summary>
    /// Thrown when video is not available and cannot be processed.
    /// </summary>
    public class VideoUnavailableException : Exception
    {
        /// <summary>
        /// ID of the video.
        /// </summary>
        public string VideoId { get; }

        /// <summary>
        /// Error code reported by YouTube.
        /// </summary>
        public int Code { get; }

        /// <summary>
        /// Error reason reported by YouTube.
        /// </summary>
        public string Reason { get; }

        /// <summary />
        public VideoUnavailableException(string videoId, int code, string reason)
            : base($"Video [{videoId}] is not available and cannot be processed. " +
                   $"Code: {code}. Reason: {reason}.")
        {
            VideoId = videoId.GuardNotNull(nameof(videoId));
            Code = code;
            Reason = reason.GuardNotNull(nameof(reason));
        }
    }
}