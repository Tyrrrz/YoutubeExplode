using System;

namespace YoutubeExplode.Exceptions
{
    /// <summary>
    /// Parent class for domain exceptions thrown by <see cref="YoutubeExplode"/>.
    /// </summary>
    public abstract class YoutubeExplodeException : Exception
    {
        /// <summary>
        /// Initializes an instance of <see cref="YoutubeExplodeException"/>.
        /// </summary>
        /// <param name="message"></param>
        protected YoutubeExplodeException(string message)
            : base(message)
        {
        }
    }
}