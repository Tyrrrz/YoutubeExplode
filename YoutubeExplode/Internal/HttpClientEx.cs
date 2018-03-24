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
            if (_singleton != null)
                return _singleton;

            var httpClientHandler = new HttpClientHandler();
            if (httpClientHandler.SupportsAutomaticDecompression)
                httpClientHandler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            httpClientHandler.UseCookies = false;

            var client = new HttpClient(httpClientHandler, true);
            client.DefaultRequestHeaders.Add("User-Agent", "YoutubeExplode (github.com/Tyrrrz/YoutubeExplode)");

            return _singleton = client;
        }

        public static async Task<string> GetStringAsync(this HttpClient httpClient, string requestUri,
            bool ensureSuccess = true)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, requestUri))
            using (var response = await httpClient.SendAsync(request).ConfigureAwait(false))
            {
                if (ensureSuccess)
                    response.EnsureSuccessStatusCode();

                return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }
        }

        public static async Task<Stream> GetStreamAsync(this HttpClient httpClient, string requestUri,
            long? from = null, long? to = null, bool ensureSuccess = true)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Range = new RangeHeaderValue(from, to);

            using (request)
            {
                var response = await httpClient.SendAsync(request).ConfigureAwait(false);

                if (ensureSuccess)
                    response.EnsureSuccessStatusCode();

                return await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            }
        }
    }
}