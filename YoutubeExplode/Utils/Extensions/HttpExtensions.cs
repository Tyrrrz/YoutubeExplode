using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace YoutubeExplode.Utils.Extensions
{
    internal static class HttpExtensions
    {
        public static async ValueTask<HttpResponseMessage> HeadAsync(
            this HttpClient httpClient,
            string requestUri,
            CancellationToken cancellationToken = default)
        {
            using var request = new HttpRequestMessage(HttpMethod.Head, requestUri);
            return await httpClient.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken
            );
        }

        public static async ValueTask<Stream> GetStreamAsync(
            this HttpClient httpClient,
            string requestUri,
            long? from = null,
            long? to = null,
            bool ensureSuccess = true,
            CancellationToken cancellationToken = default)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Range = new RangeHeaderValue(from, to);

            var response = await httpClient.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken
            );

            if (ensureSuccess)
                response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStreamAsync(cancellationToken);
        }

        public static async ValueTask<long?> TryGetContentLengthAsync(
            this HttpClient httpClient,
            string requestUri,
            bool ensureSuccess = true,
            CancellationToken cancellationToken = default)
        {
            using var response = await httpClient.HeadAsync(requestUri, cancellationToken);

            if (ensureSuccess)
                response.EnsureSuccessStatusCode();

            return response.Content.Headers.ContentLength;
        }
    }
}