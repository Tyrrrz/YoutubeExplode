using System.Net.Http;

namespace YoutubeExplode.Exceptions
{
    /// <summary>
    /// Exception thrown when YouTube denies a request because the client has exceeded rate limit.
    /// </summary>
    public partial class RequestLimitExceededException : YoutubeExplodeException
    {
        /// <summary>
        /// Initializes an instance of <see cref="RequestLimitExceededException"/>.
        /// </summary>
        public RequestLimitExceededException(string message)
            : base(message)
        {
        }
    }

    public partial class RequestLimitExceededException
    {
        internal static RequestLimitExceededException FailedHttpRequest(HttpResponseMessage response)
        {
            var message = $@"
Failed to perform an HTTP request to YouTube because of rate limiting.
This error indicates that YouTube thinks there were too many requests made from this IP and considers it suspicious.
To resolve this error, please wait some time and try again -or- try injecting an HttpClient that has cookies for an authenticated user.
Unfortunately, there's nothing the library can do to work around this error.

Request: {response.RequestMessage}

Response: {response}";

            return new RequestLimitExceededException(message.Trim());
        }
    }
}