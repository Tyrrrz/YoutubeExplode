using System;

namespace YoutubeExplode.Exceptions
{
    /// <summary>
    /// Thrown when Youtube returns an error after a query
    /// </summary>
    public class YoutubeErrorException : Exception
    {
        /// <summary>
        /// Error code
        /// </summary>
        public int ErrorCode { get; }

        /// <summary>
        /// Original error message
        /// </summary>
        public string ErrorMessage { get; }

        /// <summary>
        /// Exception messaage
        /// </summary>
        public override string Message { get; }

        internal YoutubeErrorException(int errorCode, string errorMessage)
        {
            ErrorCode = errorCode;
            ErrorMessage = errorMessage;
            Message = $"Youtube returned error {ErrorCode}:{Environment.NewLine}{ErrorMessage}";
        }
    }
}