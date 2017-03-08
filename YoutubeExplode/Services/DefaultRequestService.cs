using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using YoutubeExplode.Internal;

namespace YoutubeExplode.Services
{
    /// <summary>
    /// Uses <see cref="HttpClient"/> for handling requests
    /// </summary>
    public partial class DefaultRequestService : IRequestService, IDisposable
    {
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Creates an instance of <see cref="DefaultRequestService"/>
        /// </summary>
        public DefaultRequestService()
        {
            var httpClientHandler = new HttpClientHandler();
            if (httpClientHandler.SupportsAutomaticDecompression)
                httpClientHandler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            httpClientHandler.UseCookies = false;

            _httpClient = new HttpClient(httpClientHandler);
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "YoutubeExplode (github.com/Tyrrrz/YoutubeExplode)");
            _httpClient.DefaultRequestHeaders.Add("Connection", "Close");
        }

        /// <summary>
        /// Creates an instance of <see cref="DefaultRequestService"/> with a custom <see cref="HttpClient"/>
        /// </summary>
        public DefaultRequestService(HttpClient client)
        {
            _httpClient = client;
        }

        /// <inheritdoc />
        public virtual async Task<string> GetStringAsync(string url)
        {
            if (url.IsBlank())
                throw new ArgumentNullException(nameof(url));

            try
            {
                return await _httpClient.GetStringAsync(url).ConfigureAwait(false);
            }
            catch
            {
                return null;
            }
        }

        /// <inheritdoc />
        public virtual async Task<IDictionary<string, string>> GetHeadersAsync(string url)
        {
            if (url.IsBlank())
                throw new ArgumentNullException(nameof(url));

            try
            {
                const HttpCompletionOption compl = HttpCompletionOption.ResponseHeadersRead;
                using (var request = new HttpRequestMessage(HttpMethod.Head, url))
                using (var response = await _httpClient.SendAsync(request, compl).ConfigureAwait(false))
                    return NormalizeResponseHeaders(response);
            }
            catch
            {
                return null;
            }
        }

        /// <inheritdoc />
        public virtual async Task<Stream> GetStreamAsync(string url)
        {
            if (url.IsBlank())
                throw new ArgumentNullException(nameof(url));

            try
            {
                return await _httpClient.GetStreamAsync(url).ConfigureAwait(false);
            }
            catch
            {
                return null;
            }
        }

        /// <inheritdoc />
        public virtual void Dispose()
        {
            _httpClient.Dispose();
        }
    }

    public partial class DefaultRequestService
    {
        private static IDictionary<string, string> NormalizeResponseHeaders(HttpResponseMessage response)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var header in response.Headers)
                result.Add(header.Key, header.Value.JoinToString(" "));
            foreach (var header in response.Content.Headers)
                result.Add(header.Key, header.Value.JoinToString(" "));
            return result;
        }
    }
}
