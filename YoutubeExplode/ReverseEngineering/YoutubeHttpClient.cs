using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using YoutubeExplode.Exceptions;

namespace YoutubeExplode.ReverseEngineering
{
    internal class YoutubeHttpClient : HttpClient
    {
        private readonly HttpClient _innerHttpClient;

        public YoutubeHttpClient(HttpClient innerHttpClient)
        {
            _innerHttpClient = innerHttpClient;
        }

        public override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = await _innerHttpClient.SendAsync(request, cancellationToken);

            // Some pages redirect to https://www.google.com/sorry instead of return 429
            if (request.RequestUri.Host.EndsWith(".google.com", StringComparison.OrdinalIgnoreCase) &&
                request.RequestUri.LocalPath.StartsWith("/sorry/", StringComparison.OrdinalIgnoreCase))
                throw RequestLimitExceededException.FailedHttpRequest(request, response);

            var statusCode = (int) response.StatusCode;

            if (statusCode >= 500)
                throw TransientFailureException.FailedHttpRequest(request, response);

            if (statusCode == 429)
                throw RequestLimitExceededException.FailedHttpRequest(request, response);

            if (statusCode >= 400)
                throw FatalFailureException.FailedHttpRequest(request, response);

            return response;
        }
    }
}