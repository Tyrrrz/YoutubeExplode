using System;
using System.Net.Http;

namespace YoutubeExplode.Exceptions
{
    public partial class TransientFailureException : YoutubeExplodeException
    {
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
The status code indicates that this is most likely an error on their side and is not a bug in the library.
If this issue persists, please report it on project's GitHub.

Request: {req}

Response: {res}".Trim();

            return new TransientFailureException(message);
        }
    }
}