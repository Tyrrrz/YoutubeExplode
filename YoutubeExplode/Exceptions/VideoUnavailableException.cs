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

        /// <summary />
        public VideoUnavailableException(string videoId, string message)
            : base(message)
        {
            VideoId = videoId.GuardNotNull(nameof(videoId));
        }
    }
}