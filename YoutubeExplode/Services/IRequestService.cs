using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace YoutubeExplode.Services
{
    /// <summary>
    /// Service that performs HTTP requests for <see cref="YoutubeClient"/>
    /// </summary>
    public interface IRequestService
    {
        /// <summary>
        /// Performs a GET request and returns the response content as a string
        /// </summary>
        /// <returns>Respose content as a string or null if the operation failed</returns>
        Task<string> GetStringAsync(string url);

        /// <summary>
        /// Performs a HEAD request and returns response headers as a dictionary
        /// </summary>
        /// <returns>Header dictionary or null if the operation failed</returns>
        Task<IDictionary<string, string>> GetHeadersAsync(string url);

        /// <summary>
        /// Performs a GET request and returns the response content as a stream
        /// </summary>
        /// <returns>Response stream or null if the operation failed</returns>
        Task<Stream> GetStreamAsync(string url);
    }
}
