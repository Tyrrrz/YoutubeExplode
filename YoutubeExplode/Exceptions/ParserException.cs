using System;

namespace YoutubeExplode.Exceptions
{
    /// <summary>
    /// Thrown when a parser fails to extract required information.
    /// This usually happens when YouTube makes changes that break YoutubeExplode.
    /// </summary>
    public class ParserException : Exception
    {
        /// <summary>
        /// Initializes an instance of <see cref="ParserException"/>.
        /// </summary>
        public ParserException(string message)
            : base(message)
        {
        }
    }
}