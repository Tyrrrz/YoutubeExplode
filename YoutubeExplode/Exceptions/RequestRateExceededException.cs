using System.Net.Http;

namespace YoutubeExplode.Exceptions
{
    public partial class RequestRateExceededException : YoutubeExplodeException
    {
        public RequestRateExceededException(string message)
            : base(message)
        {
        }
    }

    public partial class RequestRateExceededException
    {
        internal static RequestRateExceededException FailedHttpRequest(HttpRequestMessage req, HttpResponseMessage res)
        {
            var message = $@"
Failed to perform an HTTP request to YouTube because the request rate has been allegedly exceeded.
This error indicates that YouTube thinks there were too many requests made from this IP and considers it suspicious.
To resolve this error, please wait some time or try injecting an HttpClient that has cookies for an authenticated user.
Unfortunately, there's nothing the library can do to work around this error.

Request: {req}

Response: {res}".Trim();

            return new RequestRateExceededException(message);
        }
    }
}