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
    /// Simple HTTP service that uses <see cref="HttpClient"/> for handling requests
    /// </summary>
    public partial class HttpService : IHttpService, IDisposable
    {
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Creates an instance of <see cref="HttpService"/> with a custom <see cref="HttpClient"/>
        /// </summary>
        public HttpService(HttpClient client)
        {
            _httpClient = client ?? throw new ArgumentNullException(nameof(client));
        }

        /// <summary>
        /// Creates an instance of <see cref="HttpService"/>
        /// </summary>
        public HttpService()
        {
            var httpClientHandler = new HttpClientHandler();
            if (httpClientHandler.SupportsAutomaticDecompression)
                httpClientHandler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            httpClientHandler.UseCookies = false;

            _httpClient = new HttpClient(httpClientHandler);
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "YoutubeExplode (github.com/Tyrrrz/YoutubeExplode)");
        }

        /// <inheritdoc />
        ~HttpService()
        {
            Dispose(false);
        }

        /// <inheritdoc />
        public virtual async Task<string> GetStringAsync(string url)
        {
            if (url == null)
                throw new ArgumentNullException(nameof(url));

            return await _httpClient.GetStringAsync(url).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public virtual async Task<IDictionary<string, string>> GetHeadersAsync(string url)
        {
            if (url == null)
                throw new ArgumentNullException(nameof(url));

            const HttpCompletionOption compl = HttpCompletionOption.ResponseHeadersRead;
            using (var request = new HttpRequestMessage(HttpMethod.Head, url))
            using (var response = await _httpClient.SendAsync(request, compl).ConfigureAwait(false))
            {
                response.EnsureSuccessStatusCode();
                return NormalizeResponseHeaders(response);
            }
        }

        /// <inheritdoc />
        public virtual async Task<Stream> GetStreamAsync(string url)
        {
            if (url == null)
                throw new ArgumentNullException(nameof(url));

            return await _httpClient.GetStreamAsync(url).ConfigureAwait(false);
        }

        /// <summary>
        /// Dispose method
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _httpClient.Dispose();
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }

    public partial class HttpService
    {
        private static HttpService _instance;

        /// <summary>
        /// Returns a reusable instance of HttpService
        /// </summary>
        public static HttpService Instance => _instance ?? (_instance = new HttpService());

        /// <summary>
        /// Converts headers returned by <see cref="HttpResponseMessage"/> into a dictionary
        /// </summary>
        protected static IDictionary<string, string> NormalizeResponseHeaders(HttpResponseMessage response)
        {
            if (response == null)
                throw new ArgumentNullException(nameof(response));

            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var header in response.Headers)
                result.Add(header.Key, header.Value.JoinToString(" "));
            foreach (var header in response.Content.Headers)
                result.Add(header.Key, header.Value.JoinToString(" "));
            return result;
        }
    }
}
