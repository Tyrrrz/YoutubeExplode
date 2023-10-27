using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace YoutubeExplode.Utils.Extensions;

internal static class HttpExtensions
{
    private class NonDisposableHttpContent : HttpContent
    {
        private readonly HttpContent _content;

        public NonDisposableHttpContent(HttpContent content) => _content = content;

        protected override async Task SerializeToStreamAsync(
            Stream stream,
            TransportContext? context
        ) => await _content.CopyToAsync(stream);

        protected override bool TryComputeLength(out long length)
        {
            length = default;
            return false;
        }
    }

    public static HttpRequestMessage Clone(this HttpRequestMessage request)
    {
        var clonedRequest = new HttpRequestMessage(request.Method, request.RequestUri)
        {
            Version = request.Version,
            // Don't dispose the original request's content
            Content = request.Content is not null
                ? new NonDisposableHttpContent(request.Content)
                : null
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

    public static async ValueTask<HttpResponseMessage> HeadAsync(
        this HttpClient http,
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

    public static async ValueTask<long?> TryGetContentLengthAsync(
        this HttpClient http,
        string requestUri,
        bool ensureSuccess = true,
        CancellationToken cancellationToken = default
    )
    {
        using var response = await http.HeadAsync(requestUri, cancellationToken);

        if (ensureSuccess)
            response.EnsureSuccessStatusCode();

        return response.Content.Headers.ContentLength;
    }
}
