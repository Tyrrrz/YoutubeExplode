using System;

namespace YoutubeExplode.Exceptions
{
    /// <summary>
    /// Thrown when Youtube's frontend returns an error
    /// </summary>
    public class YoutubeException : Exception
    {
        /// <summary>
        /// Error code
        /// </summary>
        public int Code { get; }

        internal YoutubeException(int code, string message)
            : base(message)
        {
            Code = code;
        }
    }
}