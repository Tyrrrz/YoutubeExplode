namespace YoutubeExplode.Exceptions;

/// <summary>
/// Exception thrown when YouTube denies a request because the client has exceeded rate limit.
/// </summary>
public class RequestLimitExceededException : YoutubeExplodeException
{
    /// <summary>
    /// Initializes an instance of <see cref="RequestLimitExceededException" />.
    /// </summary>
    public RequestLimitExceededException(string message) : base(message)
    {
    }
}