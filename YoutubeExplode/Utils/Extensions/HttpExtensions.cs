using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace YoutubeExplode.Utils.Extensions;

internal static class HttpExtensions
{
    private class NonDisposableHttpContent(HttpContent content) : HttpContent
    {
        protected override async Task SerializeToStreamAsync(
            Stream stream,
            TransportContext? context
        ) => await content.CopyToAsync(stream);

        protected override bool TryComputeLength(out long length)
        {
            length = 0;
            return false;
        }
    }

    extension(HttpRequestMessage request)
    {
        public HttpRequestMessage Clone()
        {
            var clonedRequest = new HttpRequestMessage(request.Method, request.RequestUri)
            {
                Version = request.Version,
                // Don't dispose the original request's content
                Content = request.Content is not null
                    ? new NonDisposableHttpContent(request.Content)
                    : null,
            };

            foreach (var (key, value) in request.Headers)
                clonedRequest.Headers.TryAddWithoutValidation(key, value);

            if (request.Content is not null && clonedRequest.Content is not null)
            {
                foreach (var (key, value) in request.Content.Headers)
                    clonedRequest.Content.Headers.TryAddWithoutValidation(key, value);
            }

            return clonedRequest;
        }
    }

    extension(HttpClient http)
    {
        public async ValueTask<HttpResponseMessage> HeadAsync(
            string requestUri,
            CancellationToken cancellationToken = default
        )
        {
            using var request = new HttpRequestMessage(HttpMethod.Head, requestUri);

            return await http.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken
            );
        }
    }
}
