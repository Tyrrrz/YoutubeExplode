namespace YoutubeExplode.Exceptions;

/// <summary>
/// Exception thrown when the requested video is unavailable.
/// </summary>
public class VideoUnavailableException : VideoUnplayableException
{
    /// <summary>
    /// Initializes an instance of <see cref="VideoUnavailableException" />.
    /// </summary>
    public VideoUnavailableException(string message) : base(message)
    {
    }
}