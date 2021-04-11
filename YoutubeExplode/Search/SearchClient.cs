using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using YoutubeExplode.Bridge;
using YoutubeExplode.Playlists;

namespace YoutubeExplode.Search
{
    /// <summary>
    /// Operations related to YouTube search.
    /// </summary>
    public class SearchClient
    {
        private readonly YoutubeController _controller;

        /// <summary>
        /// Initializes an instance of <see cref="SearchClient"/>.
        /// </summary>
        public SearchClient(HttpClient httpClient)
        {
            _controller = new YoutubeController(httpClient);
        }

        /// <summary>
        /// Enumerates the videos returned by the specified search query.
        /// </summary>
        public async IAsyncEnumerable<PlaylistVideo> GetVideosAsync(
            string searchQuery,
            int startPage,
            int pageCount,
            CancellationToken cancellationToken = default)
        {
            var continuationToken = "";
            while (true)
            {
                var searchResults =
                    await _controller.GetSearchResultsAsync(searchQuery, continuationToken, cancellationToken);
            }

            yield break;
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