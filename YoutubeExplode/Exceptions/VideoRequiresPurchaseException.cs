using YoutubeExplode.Internal;

namespace YoutubeExplode.Exceptions
{
    /// <summary>
    /// Thrown when a video is not playable because it requires purchase.
    /// </summary>
    public class VideoRequiresPurchaseException : VideoUnplayableException
    {
        /// <summary>
        /// ID of the preview video.
        /// </summary>
        public string PreviewVideoId { get; }

        /// <summary>
        /// Initializes an instance of <see cref="VideoRequiresPurchaseException"/>.
        /// </summary>
        public VideoRequiresPurchaseException(string previewVideoId, string videoId, string message)
            : base(videoId, message)
        {
            PreviewVideoId = previewVideoId.GuardNotNull(nameof(previewVideoId));
        }
    }
}