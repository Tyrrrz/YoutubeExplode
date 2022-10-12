using YoutubeExplode.Videos;

namespace YoutubeExplode.Exceptions;

/// <summary>
/// Exception thrown when the requested video requires purchase.
/// </summary>
public class VideoRequiresPurchaseException : VideoUnplayableException
{
    /// <summary>
    /// ID of a free preview video which is used as promotion for the original video.
    /// </summary>
    public VideoId PreviewVideoId { get; }

    /// <summary>
    /// Initializes an instance of <see cref="VideoRequiresPurchaseException" />
    /// </summary>
    public VideoRequiresPurchaseException(string message, VideoId previewVideoId) : base(message) =>
        PreviewVideoId = previewVideoId;
}