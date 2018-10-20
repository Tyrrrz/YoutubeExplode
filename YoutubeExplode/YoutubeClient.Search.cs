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

            // Don't ensure success here so that empty pages could be parsed

            var url = $"https://www.youtube.com/search_ajax?style=json&search_query={query}&page={page}&hl=en";
            var raw = await _httpClient.GetStringAsync(url, false).ConfigureAwait(false);

            return SearchResultsAjaxParser.Initialize(raw);
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<Video>> SearchVideosAsync(string query, int maxPages)
        {
            query.GuardNotNull(nameof(query));
            maxPages.GuardPositive(nameof(maxPages));

            // Get all videos across pages
            var videos = new List<Video>();
            for (var page = 1; page <= maxPages; page++)
            {
                // Get parser
                var parser = await GetSearchResultsAjaxParserAsync(query, page).ConfigureAwait(false);

                // Parse videos
                var countDelta = 0;
                foreach (var videoParser in parser.GetVideos())
                {
                    // Extract info
                    var videoId = videoParser.GetId();
                    var videoAuthor = videoParser.GetAuthor();
                    var videoUploadDate = videoParser.GetUploadDate();
                    var videoTitle = videoParser.GetTitle();
                    var videoDescription = videoParser.GetDescription();
                    var videoDuration = videoParser.GetDuration();
                    var videoKeywords = videoParser.GetKeywords();
                    var videoViewCount = videoParser.GetViewCount();
                    var videoLikeCount = videoParser.GetLikeCount();
                    var videoDislikeCount = videoParser.GetDislikeCount();

                    var videoStatistics = new Statistics(videoViewCount, videoLikeCount, videoDislikeCount);
                    var videoThumbnails = new ThumbnailSet(videoId);

                    var video = new Video(videoId, videoAuthor, videoUploadDate, videoTitle, videoDescription,
                        videoThumbnails, videoDuration, videoKeywords, videoStatistics);

                    videos.Add(video);
                    countDelta++;
                }

                // Break if no distinct videos were added to the list
                if (countDelta <= 0)
                    break;
            }

            return videos;
        }

        /// <inheritdoc />
        public Task<IReadOnlyList<Video>> SearchVideosAsync(string query)
            => SearchVideosAsync(query, int.MaxValue);
    }
}