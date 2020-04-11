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
        internal static FatalFailureException FailedHttpRequest(HttpRequestMessage req, HttpResponseMessage res)
        {
            var message = $@"
Failed to perform an HTTP request to YouTube.
The response indicates that YouTube most likely changed something which broke the library.
If this issue persists, please report it on the project's GitHub page.

Request: {req}

Response: {res}".Trim();

            return new FatalFailureException(message);
        }

        internal static TransientFailureException Generic(string unmetExpectations)
        {
            var message = $@"
{unmetExpectations}
This error is most likely caused by a change on YouTube's side which broke the library.
If this issue persists, please report it on the project's GitHub page.".Trim();

            return new TransientFailureException(message);
        }
    }
}