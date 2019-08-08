using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using YoutubeExplode.Internal;
using YoutubeExplode.Models;

namespace YoutubeExplode
{
    public partial class YoutubeClient
    {
        private async Task<JToken> GetSearchResultsJsonAsync(string query, int page)
        {
            var url = $"https://youtube.com/search_ajax?style=json&search_query={query.UrlEncode()}&page={page}&hl=en";
            var raw = await _httpClient.GetStringAsync(url, false).ConfigureAwait(false); // don't ensure success but rather return empty list

            return JToken.Parse(raw);
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
                // Get search results JSON
                var resultsJson = await GetSearchResultsJsonAsync(query, page).ConfigureAwait(false);

                // Get videos
                var countDelta = 0;
                foreach (var videoJson in resultsJson.SelectToken("video").EmptyIfNull())
                {
                    var epoch = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);

                    // Extract video info
                    var videoId = videoJson.SelectToken("encrypted_id").Value<string>();
                    var videoAuthor = videoJson.SelectToken("author").Value<string>();
                    var videoUploadDate = epoch + TimeSpan.FromSeconds(videoJson.SelectToken("time_created").Value<long>());
                    var videoTitle = videoJson.SelectToken("title").Value<string>();
                    var videoDescription = videoJson.SelectToken("description").Value<string>();
                    var videoDuration = TimeSpan.FromSeconds(videoJson.SelectToken("length_seconds").Value<double>());
                    var videoViewCount = videoJson.SelectToken("views").Value<string>().StripNonDigit().ParseLong();
                    var videoLikeCount = videoJson.SelectToken("likes").Value<long>();
                    var videoDislikeCount = videoJson.SelectToken("dislikes").Value<long>();

                    // Extract video keywords
                    var videoKeywordsJoined = videoJson.SelectToken("keywords").Value<string>();
                    var videoKeywords = Regex.Matches(videoKeywordsJoined, "\"[^\"]+\"|\\S+")
                        .Cast<Match>()
                        .Select(m => m.Value)
                        .Where(s => !s.IsNullOrWhiteSpace())
                        .Select(s => s.Trim('"'))
                        .ToArray();

                    // Create statistics and thumbnails
                    var videoStatistics = new Statistics(videoViewCount, videoLikeCount, videoDislikeCount);
                    var videoThumbnails = new ThumbnailSet(videoId);

                    // Add to list
                    videos.Add(new Video(videoId, videoAuthor, videoUploadDate, videoTitle, videoDescription,
                        videoThumbnails, videoDuration, videoKeywords, videoStatistics));

                    // Increment delta
                    countDelta++;
                }

                // If no distinct videos were added to the list - break
                if (countDelta <= 0)
                    break;
            }

            return videos;
        }

        /// <inheritdoc />
        public Task<IReadOnlyList<Video>> SearchVideosAsync(string query) => SearchVideosAsync(query, int.MaxValue);
    }
}