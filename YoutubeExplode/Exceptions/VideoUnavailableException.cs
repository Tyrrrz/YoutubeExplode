using System;
using YoutubeExplode.Internal;

namespace YoutubeExplode.Exceptions
{
    /// <summary>
    /// Thrown when a video is not available and cannot be processed.
    /// This can happen because the video does not exist, is deleted, is private, or due to other reasons.
    /// </summary>
    public class VideoUnavailableException : Exception
    {
        /// <summary>
        /// ID of the video.
        /// </summary>
        public string VideoId { get; }

        /// <summary>
        /// Initializes an instance of <see cref="VideoUnavailableException"/>.
        /// </summary>
        public VideoUnavailableException(string videoId, string message)
            : base(message)
        {
            VideoId = videoId.GuardNotNull(nameof(videoId));
        }
    }
}