using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace YoutubeExplode.Services
{
    /// <summary>
    /// Service extensions
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Sends a GET request and returns content as a string
        /// </summary>
        public static async Task<string> GetStringAsync(this IHttpService httpService, string url)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, url))
            using (var response = await httpService.PerformRequestAsync(request).ConfigureAwait(false))
            {
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Send a GET request and return content as a stream
        /// </summary>
        public static async Task<Stream> GetStreamAsync(this IHttpService httpService, string url)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, url))
            {
                var response = await httpService.PerformRequestAsync(request).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            }
        }
    }
}