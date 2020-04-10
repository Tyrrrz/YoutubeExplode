using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace YoutubeExplode.Internal.Extensions
{
    internal static class HttpClientExtensions
    {
        public static async Task<HttpResponseMessage> HeadAsync(this HttpClient client, string requestUri)
        {
            using var request = new HttpRequestMessage(HttpMethod.Head, requestUri);
            return await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
        }

        public static async Task<string> GetStringAsync(this HttpClient client, string requestUri, bool ensureSuccess = true)
        {
            using var response = await client.GetAsync(requestUri);

            if (ensureSuccess)
                response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

        public static async Task<Stream> GetStreamAsync(this HttpClient client, string requestUri,
            long? from = null, long? to = null, bool ensureSuccess = true)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Range = new RangeHeaderValue(from, to);

            using (request)
            {
                var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

                if (ensureSuccess)
                    response.EnsureSuccessStatusCode();

                return await response.Content.ReadAsStreamAsync();
            }
        }

        public static async Task<long?> TryGetContentLengthAsync(this HttpClient client, string requestUri,
            bool ensureSuccess = true)
        {
            using var response = await client.HeadAsync(requestUri);

            if (ensureSuccess)
                response.EnsureSuccessStatusCode();

            return response.Content.Headers.ContentLength;
        }
    }
}