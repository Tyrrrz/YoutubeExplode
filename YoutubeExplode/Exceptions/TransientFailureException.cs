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
        internal static TransientFailureException FailedHttpRequest(HttpRequestMessage req, HttpResponseMessage res)
        {
            var message = $@"
Failed to perform an HTTP request to YouTube.
The response indicates that this is most likely an error on YouTube's side and is not a bug in the library.
If this issue persists, please report it on the project's GitHub page.

Request: {req}

Response: {res}".Trim();

            return new TransientFailureException(message);
        }

        internal static TransientFailureException Generic(string unmetExpectations)
        {
            var message = $@"
{unmetExpectations}
This error is most likely caused by a transient failure on YouTube's side and is not a bug in the library.
If this issue persists, please report it on the project's GitHub page.".Trim();

            return new TransientFailureException(message);
        }
    }
}