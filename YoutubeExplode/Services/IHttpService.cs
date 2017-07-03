using System.Net.Http;
using System.Threading.Tasks;

namespace YoutubeExplode.Services
{
    /// <summary>
    /// Performs HTTP requests
    /// </summary>
    public interface IHttpService
    {
        /// <summary>
        /// Performs a generic HTTP request
        /// </summary>
        Task<HttpResponseMessage> PerformRequestAsync(HttpRequestMessage request,
            HttpCompletionOption completionOption = HttpCompletionOption.ResponseHeadersRead);
    }
}
