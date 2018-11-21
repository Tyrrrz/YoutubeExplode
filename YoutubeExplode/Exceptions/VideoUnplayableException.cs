using System;
using YoutubeExplode.Internal;

namespace YoutubeExplode.Exceptions
{
    /// <summary>
    /// Thrown when a video is not playable and its streams cannot be resolved.
    /// This can happen because the video requires purchase, is blocked in your country, is controversial, or due to other reasons.
    /// </summary>
    public class VideoUnplayableException : Exception
    {
        /// <summary>
        /// ID of the video.
        /// </summary>
        public string VideoId { get; }

        /// <summary>
        /// Initializes an instance of <see cref="VideoUnplayableException"/>.
        /// </summary>
        public VideoUnplayableException(string videoId, string message)
            : base(message)
        {
            VideoId = videoId.GuardNotNull(nameof(videoId));
        }
    }
}