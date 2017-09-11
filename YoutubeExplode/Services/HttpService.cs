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
        /// <summary>
        /// Internal instance of <see cref="HttpClient"/>
        /// </summary>
        protected HttpClient Client { get; }

        /// <summary>
        /// Creates an instance of <see cref="HttpService"/> with a custom <see cref="HttpClient"/>
        /// </summary>
        public HttpService(HttpClient client)
        {
            Client = client ?? throw new ArgumentNullException(nameof(client));
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

            Client = new HttpClient(httpClientHandler);
            Client.DefaultRequestHeaders.Add("User-Agent", "YoutubeExplode (github.com/Tyrrrz/YoutubeExplode)");
        }

        /// <inheritdoc />
        ~HttpService()
        {
            Dispose(false);
        }

        /// <inheritdoc />
        public virtual async Task<HttpResponseMessage> PerformRequestAsync(HttpRequestMessage request)
        {
            return await Client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
        }

        /// <summary>
        /// Dispose method
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Client.Dispose();
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
        /// Singleton instance of <see cref="HttpService"/>
        /// </summary>
        public static HttpService Instance => _instance ?? (_instance = new HttpService());
    }
}