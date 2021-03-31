using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace YoutubeExplode.Utils.Extensions
{
    internal static class HttpClientExtensions
    {
        public static async ValueTask<HttpResponseMessage> HeadAsync(
            this HttpClient httpClient,
            string requestUri)
        {
            using var request = new HttpRequestMessage(HttpMethod.Head, requestUri);
            return await httpClient.SendAsync(request);
        }

        public static async ValueTask<string> GetStringAsync(
            this HttpClient httpClient,
            string requestUri,
            bool ensureSuccess = true)
        {
            using var response = await httpClient.GetAsync(requestUri);

            if (ensureSuccess)
                response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

        public static async ValueTask<Stream> GetStreamAsync(
            this HttpClient httpClient,
            string requestUri,
            long? from = null,
            long? to = null,
            bool ensureSuccess = true)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Range = new RangeHeaderValue(from, to);

            var response = await httpClient.SendAsync(request);

            if (ensureSuccess)
                response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStreamAsync();
        }

        public static async ValueTask<long?> TryGetContentLengthAsync(
            this HttpClient httpClient,
            string requestUri,
            bool ensureSuccess = true)
        {
            using var response = await httpClient.HeadAsync(requestUri);

            if (ensureSuccess)
                response.EnsureSuccessStatusCode();

            return response.Content.Headers.ContentLength;
        }
    }
}