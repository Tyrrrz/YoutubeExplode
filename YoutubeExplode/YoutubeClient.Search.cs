using System.Collections.Generic;
using System.Threading.Tasks;
using YoutubeExplode.Internal;
using YoutubeExplode.Internal.Parsers;
using YoutubeExplode.Models;

namespace YoutubeExplode
{
    public partial class YoutubeClient
    {
        private async Task<SearchResultsAjaxParser> GetSearchResultsAjaxParserAsync(string query, int page)
        {
            query = query.UrlEncode();

            var url = $"https://www.youtube.com/search_ajax?style=json&search_query={query}&page={page}&hl=en";
            var raw = await _httpClient.GetStringAsync(url).ConfigureAwait(false);

            return SearchResultsAjaxParser.Initialize(raw);
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<Video>> SearchVideosAsync(string query, int maxPages)
        {
            query.GuardNotNull(nameof(query));
            maxPages.GuardPositive(nameof(maxPages));

            // Get all videos across pages
            var videos = new List<Video>();
            for (var i = 1; i <= maxPages; i++)
            {
                // Get search results
                var parser = await GetSearchResultsAjaxParserAsync(query, i).ConfigureAwait(false);

                // Parse videos
                foreach (var videoParser in parser.Videos())
                {
                    // Basic info
                    var videoId = videoParser.GetId();
                    var videoAuthor = videoParser.GetAuthor();
                    var videoUploadDate = videoParser.GetUploadDate();
                    var videoTitle = videoParser.GetTitle();
                    var videoDuration = videoParser.GetDuration();
                    var videoDescription = videoParser.GetDescription();
                    var videoKeywords = videoParser.GetKeywords();

                    // Statistics
                    var videoViewCount = videoParser.GetViewCount();
                    var videoLikeCount = videoParser.GetLikeCount();
                    var videoDislikeCount = videoParser.GetDislikeCount();

                    // Video
                    var videoStatistics = new Statistics(videoViewCount, videoLikeCount, videoDislikeCount);
                    var videoThumbnails = new ThumbnailSet(videoId);
                    var video = new Video(videoId, videoAuthor, videoUploadDate, videoTitle, videoDescription,
                        videoThumbnails, videoDuration, videoKeywords, videoStatistics);

                    videos.Add(video);
                }
            }

            return videos;
        }

        /// <inheritdoc />
        public Task<IReadOnlyList<Video>> SearchVideosAsync(string query)
            => SearchVideosAsync(query, int.MaxValue);
    }
}