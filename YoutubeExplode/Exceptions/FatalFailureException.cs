using System.Net.Http;

namespace YoutubeExplode.Exceptions
{
    public partial class FatalFailureException : YoutubeExplodeException
    {
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
The status code indicates that YouTube most likely changed something and broke this library.
If this issue persists, please report it on the project's GitHub page.

Request: {req}

Response: {res}".Trim();

            return new FatalFailureException(message);
        }
    }
}