using System;

namespace YoutubeExplode.Exceptions
{
    /// <summary>
    /// Thrown when Youtube's frontend returns an error when getting video info
    /// </summary>
    public class VideoNotAvailableException : Exception
    {
        /// <summary>
        /// Error code
        /// </summary>
        public int Code { get; }

        /// <inheritdoc />
        public VideoNotAvailableException(int code, string message)
            : base(message)
        {
            Code = code;
        }
    }
}