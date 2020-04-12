namespace YoutubeExplode.Exceptions
{
    /// <summary>
    /// Exception thrown when the requested video is unavailable.
    /// </summary>
    public partial class VideoUnavailableException : YoutubeExplodeException
    {
        /// <summary>
        /// Initializes an instance of <see cref="VideoUnavailableException"/>.
        /// </summary>
        public VideoUnavailableException(string message)
            : base(message)
        {
        }
    }

    public partial class VideoUnavailableException
    {
        internal static VideoUnavailableException Unplayable(string videoId) => new VideoUnavailableException(
            $"Video '{videoId}' is unplayable. " +
            "We can't get streaming data for this video. " +
            "Other information such as general metadata may still be available. " +
            "In most cases, this error indicates that this video is blocked in your country."
        );

        internal static VideoUnavailableException Unavailable(string videoId) => new VideoUnavailableException(
            $"Video '{videoId}' is unavailable. " +
            "We can't get streaming data or any other information for this video. " +
            "In most cases, this error indicates that the video doesn't exist, is private, or has been taken down. " +
            "If you can open this video in your browser in incognito mode, it's most likely means that YouTube changed something that broke this library. " +
            "Please report this issue on GitHub in that case."
        );

        internal static VideoUnavailableException LiveStream(string videoId) => new VideoUnavailableException(
            $"Video '{videoId}' is an ongoing live stream. " +
            "We can't get streaming data for this video. " +
            "Please wait until the live stream finishes and try again."
        );

        internal static VideoUnavailableException NotLiveStream(string videoId) => new VideoUnavailableException(
            $"Video '{videoId}' is not an ongoing live stream. " +
            "We can't get live streaming data for this video."
        );
    }
}