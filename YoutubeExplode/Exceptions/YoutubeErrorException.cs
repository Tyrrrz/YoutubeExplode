using System;

namespace YoutubeExplode.Exceptions
{
    /// <summary>
    /// Thrown when Youtube's frontend returns an error
    /// </summary>
    public class YoutubeErrorException : Exception
    {
        /// <summary>
        /// Error code
        /// </summary>
        public int Code { get; }

        internal YoutubeErrorException(int code, string message)
            : base(message)
        {
            Code = code;
        }
    }
}