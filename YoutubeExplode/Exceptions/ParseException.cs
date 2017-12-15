using System;

namespace YoutubeExplode.Exceptions
{
    /// <summary>
    /// Thrown when there was an error parsing some data.
    /// </summary>
    public class ParseException : Exception
    {
        /// <summary />
        public ParseException(string message)
            : base(message)
        {
        }
    }
}