using System.Net.Http;

namespace YoutubeExplode.Exceptions
{
    /// <summary>
    /// Exception thrown when a transient failure occurs.
    /// </summary>
    public partial class TransientFailureException : YoutubeExplodeException
    {
        /// <summary>
        /// Initializes an instance of <see cref="TransientFailureException"/>.
        /// </summary>
        public TransientFailureException(string message)
            : base(message)
        {
        }
    }

    public partial class TransientFailureException
    {
        internal static TransientFailureException FailedHttpRequest(HttpResponseMessage response)
        {
            var message = $@"
Failed to perform an HTTP request to YouTube due to a transient failure.
In most cases, this error indicates that the problem is on YouTube's side and this is not a bug in the library.
To resolve this error, please wait some time and try again.
If this issue persists, please report it on the project's GitHub page.

Request: {response.RequestMessage}

Response: {response}";

            return new TransientFailureException(message.Trim());
        }

        internal static TransientFailureException Generic(string? reason = null)
        {
            var message = $@"
{reason}
In most cases, this error indicates that the problem is on YouTube's side and this is not a bug in the library.
To resolve this error, please wait some time and try again.
If this issue persists, please report it on the project's GitHub page.";

            return new TransientFailureException(message.Trim());
        }
    }
}