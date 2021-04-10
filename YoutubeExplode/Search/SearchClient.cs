using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using YoutubeExplode.Playlists;

namespace YoutubeExplode.Search
{
    /// <summary>
    /// Operations related to YouTube search.
    /// </summary>
    public class SearchClient
    {
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Initializes an instance of <see cref="SearchClient"/>.
        /// </summary>
        public SearchClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        /// <summary>
        /// Enumerates the videos returned by the specified search query.
        /// </summary>
        public IAsyncEnumerable<PlaylistVideo> GetVideosAsync(
            string searchQuery,
            int startPage,
            int pageCount,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Enumerates videos returned by the specified search query.
        /// </summary>
        // This needs to be an overload to maintain backwards compatibility
        public IAsyncEnumerable<PlaylistVideo> GetVideosAsync(
            string searchQuery,
            CancellationToken cancellationToken = default) =>
            GetVideosAsync(searchQuery, 0, int.MaxValue, cancellationToken);
    }
}