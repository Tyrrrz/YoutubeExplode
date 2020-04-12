using YoutubeExplode.Videos;

namespace YoutubeExplode.Exceptions
{
    /// <summary>
    /// Exception thrown when the requested video is unplayable.
    /// </summary>
    public partial class VideoUnplayableException : YoutubeExplodeException
    {
        /// <summary>
        /// Initializes an instance of <see cref="VideoUnplayableException"/>.
        /// </summary>
        public VideoUnplayableException(string message)
            : base(message)
        {
        }
    }

    public partial class VideoUnplayableException
    {
        internal static VideoUnplayableException Unplayable(VideoId videoId, string? reason = null)
        {
            var message = $@"
Video '{videoId}' is unplayable.
Streams are not available for this video.
In most cases, this error indicates that there are some restrictions in place that prevent watching this video.

Reason: {reason}";

            return new VideoUnplayableException(message.Trim());
        }

        internal static VideoUnplayableException LiveStream(VideoId videoId)
        {
            var message = $@"
Video '{videoId}' is an ongoing live stream.
Streams are not available for this video.
Please wait until the live stream finishes and try again.";

            return new VideoUnplayableException(message.Trim());
        }

        internal static VideoUnplayableException NotLiveStream(VideoId videoId)
        {
            var message = $@"
Video '{videoId}' is not an ongoing live stream.
Live stream manifest is not available for this video.";

            return new VideoUnplayableException(message.Trim());
        }
    }
}