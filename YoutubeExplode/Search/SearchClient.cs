using System;
using System.Collections.Generic;
using System.Net.Http;
using YoutubeExplode.Playlists;

namespace YoutubeExplode.Search
{
    /// <summary>
    /// YouTube search queries.
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
        /// <param name="searchQuery">The term to look for.</param>
        /// <param name="startPage">Sets how many page should be skipped from the beginning of the search.</param>
        /// <param name="pageCount">Limits how many page should be requested to complete the search.</param>
        public IAsyncEnumerable<PlaylistVideo> GetVideosAsync(string searchQuery, int startPage, int pageCount)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Enumerates the videos returned by the specified search query.
        /// </summary>
        // This needs to be an overload to maintain backwards compatibility
        public IAsyncEnumerable<PlaylistVideo> GetVideosAsync(string searchQuery) =>
            GetVideosAsync(searchQuery, 0, int.MaxValue);
    }
}