using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace YoutubeExplode.Services
{
    /// <summary>
    /// Performs HTTP requests
    /// </summary>
    public interface IHttpService
    {
        /// <summary>
        /// Performs a GET request and returns the response content as a string
        /// </summary>
        Task<string> GetStringAsync(string url);

        /// <summary>
        /// Performs a HEAD request and returns response headers as a dictionary
        /// </summary>
        Task<IDictionary<string, string>> GetHeadersAsync(string url);

        /// <summary>
        /// Performs a GET request and returns the response content as a stream
        /// </summary>
        Task<Stream> GetStreamAsync(string url);
    }
}
