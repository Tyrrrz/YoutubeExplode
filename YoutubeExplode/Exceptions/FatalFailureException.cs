using System.Net.Http;

namespace YoutubeExplode.Exceptions
{
    /// <summary>
    /// Exception thrown when a fatal failure occurs.
    /// </summary>
    public partial class FatalFailureException : YoutubeExplodeException
    {
        /// <summary>
        /// Initializes an instance of <see cref="FatalFailureException"/>.
        /// </summary>
        public FatalFailureException(string message)
            : base(message)
        {
        }
    }

    public partial class FatalFailureException
    {
        internal static FatalFailureException FailedHttpRequest(HttpResponseMessage response)
        {
            var message = $@"
Failed to perform an HTTP request to YouTube due to a fatal failure.
In most cases, this error indicates that YouTube most likely changed something, which broke the library.
If this issue persists, please report it on the project's GitHub page.

{response.RequestMessage}

{response}";

            return new FatalFailureException(message.Trim());
        }

        internal static TransientFailureException Generic(string? reason = null)
        {
            var message = $@"
{reason}
In most cases, this error indicates that YouTube most likely changed something, which broke the library.
If this issue persists, please report it on the project's GitHub page.";

            return new TransientFailureException(message.Trim());
        }
    }
}