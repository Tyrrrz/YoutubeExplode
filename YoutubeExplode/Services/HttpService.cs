using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

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
        public async Task<HttpResponseMessage> PerformRequestAsync(HttpRequestMessage request,
            HttpCompletionOption completionOption = HttpCompletionOption.ResponseHeadersRead)
        {
            return await _httpClient.SendAsync(request, completionOption).ConfigureAwait(false);
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
    }
}
