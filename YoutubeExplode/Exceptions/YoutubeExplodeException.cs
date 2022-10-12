using System;

namespace YoutubeExplode.Exceptions;

/// <summary>
/// Exception thrown within <see cref="YoutubeExplode" />.
/// </summary>
public class YoutubeExplodeException : Exception
{
    /// <summary>
    /// Initializes an instance of <see cref="YoutubeExplodeException" />.
    /// </summary>
    /// <param name="message"></param>
    public YoutubeExplodeException(string message) : base(message)
    {
    }
}