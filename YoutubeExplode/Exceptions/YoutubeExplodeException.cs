using System;

namespace YoutubeExplode.Exceptions
{
    /// <summary>
    /// Domain exception thrown by <see cref="YoutubeExplode"/>.
    /// </summary>
    public class YoutubeExplodeException : Exception
    {
        /// <summary>
        /// Initializes an instance of <see cref="YoutubeExplodeException"/>.
        /// </summary>
        /// <param name="message"></param>
        public YoutubeExplodeException(string message) : base(message)
        {
        }
    }
}