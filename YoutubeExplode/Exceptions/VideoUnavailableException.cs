using YoutubeExplode.Videos;

namespace YoutubeExplode.Exceptions
{
    /// <summary>
    /// Exception thrown when the requested video is unavailable.
    /// </summary>
    public partial class VideoUnavailableException : VideoUnplayableException
    {
        /// <summary>
        /// Initializes an instance of <see cref="VideoUnavailableException"/>.
        /// </summary>
        public VideoUnavailableException(string message) : base(message)
        {
        }
    }

    public partial class VideoUnavailableException
    {
        internal static VideoUnavailableException Unavailable(VideoId videoId)
        {
            var message = $@"
Video '{videoId}' is unavailable.
In most cases, this error indicates that the video doesn't exist, is private, or has been taken down.
If you can however open this video in your browser in incognito mode, it most likely means that YouTube changed something, which broke this library.
Please report this issue on GitHub in that case.";

            return new VideoUnavailableException(message.Trim());
        }
    }
}