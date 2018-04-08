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
        private async Task<string> GetSearchResultsRawAsync(string query, int page = 1)
        {
            query = query.UrlEncode();
            var url = $"https://www.youtube.com/search_ajax?style=json&search_query={query}&page={page}&hl=en";
            return await _httpClient.GetStringAsync(url, false).ConfigureAwait(false);
        }

        private async Task<JToken> GetSearchResultsAsync(string query, int page = 1)
        {
            var raw = await GetSearchResultsRawAsync(query, page).ConfigureAwait(false);
            return JToken.Parse(raw);
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
                var searchResultsJson = await GetSearchResultsAsync(query, i).ConfigureAwait(false);

                // Get videos
                var videosJson = searchResultsJson["video"].EmptyIfNull().ToArray();

                // Break if there are no videos
                if (!videosJson.Any())
                    break;

                // Parse videos
                foreach (var videoJson in videosJson)
                {
                    // Basic info
                    var videoId = videoJson["encrypted_id"].Value<string>();
                    var videoAuthor = videoJson["author"].Value<string>();
                    var videoUploadDate = videoJson["added"].Value<string>().ParseDateTimeOffset("M/d/yy");
                    var videoTitle = videoJson["title"].Value<string>();
                    var videoDuration = TimeSpan.FromSeconds(videoJson["length_seconds"].Value<double>());
                    var videoDescription = videoJson["description"].Value<string>().HtmlDecode();

                    // Keywords
                    var videoKeywordsJoined = videoJson["keywords"].Value<string>();
                    var videoKeywords = Regex
                        .Matches(videoKeywordsJoined, @"(?<=(^|\s)(?<q>""?))([^""]|(""""))*?(?=\<q>(?=\s|$))")
                        .Cast<Match>()
                        .Select(m => m.Value)
                        .Where(s => s.IsNotBlank())
                        .ToArray();

                    // Statistics
                    var videoViewCount = videoJson["views"].Value<string>().StripNonDigit().ParseLong();
                    var videoLikeCount = videoJson["likes"].Value<long>();
                    var videoDislikeCount = videoJson["dislikes"].Value<long>();
                    var videoStatistics = new Statistics(videoViewCount, videoLikeCount, videoDislikeCount);

                    // Video
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