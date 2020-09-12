using System.Collections.Generic;
using YoutubeExplode.Common;
using YoutubeExplode.ReverseEngineering;
using YoutubeExplode.ReverseEngineering.Responses;
using YoutubeExplode.Videos;

namespace YoutubeExplode.Search
{
    /// <summary>
    /// YouTube search queries.
    /// </summary>
    public class SearchClient
    {
        private readonly YoutubeHttpClient _httpClient;

        /// <summary>
        /// Initializes an instance of <see cref="SearchClient"/>.
        /// </summary>
        internal SearchClient(YoutubeHttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        /// <summary>
        /// Enumerates videos returned by the specified search query.
        /// </summary>
        /// <param name="searchQuery">The term to look for.</param>
        /// <param name="startPage">Sets how many page should be skipped from the beginning of the search.</param>
        /// <param name="pageCount">Limits how many page should be requested to complete the search.</param>
        public async IAsyncEnumerable<Video> GetVideosAsync(string searchQuery, int startPage, int pageCount)
        {
            var encounteredVideoIds = new HashSet<string>();
            
            for (var page = startPage; page < startPage + pageCount; page++)
            {
                var response = await PlaylistResponse.GetSearchResultsAsync(_httpClient, searchQuery, page);

                var countDelta = 0;
                foreach (var video in response.GetVideos())
                {
                    var videoId = video.GetId();

                    // Skip already encountered videos
                    if (!encounteredVideoIds.Add(videoId))
                        continue;

                    yield return new Video(
                        videoId,
                        video.GetTitle(),
                        video.GetAuthor(),
                        video.GetChannelId(),
                        video.GetUploadDate(),
                        video.GetDescription(),
                        video.GetDuration(),
                        new ThumbnailSet(videoId),
                        video.GetKeywords(),
                        new Engagement(
                            video.GetViewCount(),
                            video.GetLikeCount(),
                            video.GetDislikeCount()
                        )
                    );

                    countDelta++;
                }

                // Videos loop around, so break when we stop seeing new videos
                if (countDelta <= 0)
                    break;
            }
        }

        /// <summary>
        /// Enumerates videos returned by the specified search query.
        /// </summary>
        // This needs to be an overload to maintain backwards compatibility
        public IAsyncEnumerable<Video> GetVideosAsync(string searchQuery) =>
            GetVideosAsync(searchQuery, 0, int.MaxValue);
    }
}
