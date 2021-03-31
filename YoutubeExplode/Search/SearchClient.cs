using System.Collections.Generic;
using System.Net.Http;
using YoutubeExplode.Common;
using YoutubeExplode.Playlists;
using YoutubeExplode.ReverseEngineering;
using YoutubeExplode.ReverseEngineering.Responses;

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
        /// Enumerates videos returned by the specified search query.
        /// </summary>
        /// <param name="searchQuery">The term to look for.</param>
        /// <param name="startPage">Sets how many page should be skipped from the beginning of the search.</param>
        /// <param name="pageCount">Limits how many page should be requested to complete the search.</param>
        public async IAsyncEnumerable<PlaylistVideo> GetVideosAsync(string searchQuery, int startPage, int pageCount)
        {
            var encounteredVideoIds = new HashSet<string>();
            var continuationToken = "";

            for (var page = 0; page < startPage + pageCount; page++)
            {
                var response = await PlaylistResponse.GetSearchResultsAsync(_httpClient, searchQuery, continuationToken);

                if (page >= startPage)
                {
                    foreach (var video in response.GetVideos())
                    {
                        var videoId = video.GetId();

                        // Skip already encountered videos
                        if (!encounteredVideoIds.Add(videoId))
                            continue;

                        yield return new PlaylistVideo(
                            videoId,
                            video.GetTitle(),
                            video.GetAuthor(),
                            video.GetChannelId(),
                            video.GetDescription(),
                            video.GetDuration(),
                            video.GetViewCount(),
                            new ThumbnailSet(videoId)
                        );
                    }
                }

                continuationToken = response.TryGetContinuationToken();
                if (string.IsNullOrEmpty(continuationToken))
                    break;
            }
        }

        /// <summary>
        /// Enumerates videos returned by the specified search query.
        /// </summary>
        // This needs to be an overload to maintain backwards compatibility
        public IAsyncEnumerable<PlaylistVideo> GetVideosAsync(string searchQuery) =>
            GetVideosAsync(searchQuery, 0, int.MaxValue);
    }
}