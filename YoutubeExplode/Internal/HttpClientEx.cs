using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace YoutubeExplode.Internal
{
    internal static class HttpClientEx
    {
        private static HttpClient _singleton;

        public static HttpClient GetSingleton()
        {
            // Return cached singleton if already initialized
            if (_singleton != null)
                return _singleton;

            // Configure handler
            var handler = new HttpClientHandler();
            if (handler.SupportsAutomaticDecompression)
                handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            handler.UseCookies = false;

            // Configure client
            var client = new HttpClient(handler, true);
            //client.DefaultRequestHeaders.Add("User-Agent", "YoutubeExplode (github.com/Tyrrrz/YoutubeExplode)");

            return _singleton = client;
        }

        public static async Task<HttpResponseMessage> HeadAsync(this HttpClient client, string requestUri)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Head, requestUri))
                return await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
        }

        public static async Task<string> GetStringAsync(this HttpClient client, string requestUri,
            bool ensureSuccess = true)
        {
            using (var response = await client.GetAsync(requestUri, HttpCompletionOption.ResponseHeadersRead)
                .ConfigureAwait(false))
            {
                if (ensureSuccess)
                    response.EnsureSuccessStatusCode();

                return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }
        }

        public static async Task<Stream> GetStreamAsync(this HttpClient client, string requestUri,
            long? from = null, long? to = null, bool ensureSuccess = true)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Range = new RangeHeaderValue(from, to);

            using (request)
            {
                var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead)
                    .ConfigureAwait(false);

                if (ensureSuccess)
                    response.EnsureSuccessStatusCode();

                return await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            }
        }

        public static async Task<long?> GetContentLengthAsync(this HttpClient client, string requestUri,
            bool ensureSuccess = true)
        {
            using (var response = await client.HeadAsync(requestUri).ConfigureAwait(false))
            {
                if (ensureSuccess)
                    response.EnsureSuccessStatusCode();

                return response.Content.Headers.ContentLength;
            }
        }

        public static SegmentedHttpStream CreateSegmentedStream(this HttpClient httpClient, string url, long length,
            long segmentSize)
        {
            return new SegmentedHttpStream(httpClient, url, length, segmentSize);
        }
    }
}