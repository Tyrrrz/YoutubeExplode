using System.Net.Http;
using System.Threading.Tasks;

namespace YoutubeExplode.Services
{
    /// <summary>
    /// Provider for executing HTTP requests.
    /// </summary>
    public interface IHttpService
    {
        /// <summary>
        /// Executes a generic HTTP request.
        /// </summary>
        Task<HttpResponseMessage> PerformRequestAsync(HttpRequestMessage request);
    }
}