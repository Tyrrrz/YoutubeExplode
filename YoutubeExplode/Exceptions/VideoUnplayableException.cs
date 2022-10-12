namespace YoutubeExplode.Exceptions;

/// <summary>
/// Exception thrown when the requested video is unplayable.
/// </summary>
public class VideoUnplayableException : YoutubeExplodeException
{
    /// <summary>
    /// Initializes an instance of <see cref="VideoUnplayableException" />.
    /// </summary>
    public VideoUnplayableException(string message) : base(message)
    {
    }
}