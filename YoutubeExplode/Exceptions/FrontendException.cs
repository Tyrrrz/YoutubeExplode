using System;

namespace YoutubeExplode.Exceptions
{
    /// <summary>
    /// Thrown when Youtube's frontend returns an error
    /// </summary>
    public class FrontendException : Exception
    {
        /// <summary>
        /// Error code
        /// </summary>
        public int Code { get; }

        /// <inheritdoc />
        public FrontendException(int code, string message)
            : base(message)
        {
            Code = code;
        }
    }
}