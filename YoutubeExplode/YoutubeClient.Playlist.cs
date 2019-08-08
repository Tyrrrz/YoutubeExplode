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
        private async Task<JToken> GetPlaylistJsonAsync(string playlistId, int index)
        {
            var url = $"https://youtube.com/list_ajax?style=json&action_get_list=1&list={playlistId}&index={index}&hl=en";
            var raw = await _httpClient.GetStringAsync(url).ConfigureAwait(false);

            return JToken.Parse(raw);
        }

        /// <inheritdoc />
        public async Task<Playlist> GetPlaylistAsync(string playlistId, int maxPages)
        {
            playlistId.GuardNotNull(nameof(playlistId));
            maxPages.GuardPositive(nameof(maxPages));

            if (!ValidatePlaylistId(playlistId))
                throw new ArgumentException($"Invalid YouTube playlist ID [{playlistId}].", nameof(playlistId));

            // Get all videos across pages
            JToken playlistJson;
            var page = 1;
            var index = playlistId.StartsWith("PL", StringComparison.OrdinalIgnoreCase) ? 101 : 0;
            var videoIds = new HashSet<string>();
            var videos = new List<Video>();
            do
            {
                // Get playlist JSON
                playlistJson = await GetPlaylistJsonAsync(playlistId, index).ConfigureAwait(false);

                // Get videos
                var countDelta = 0;
                foreach (var videoJson in playlistJson.SelectToken("video").EmptyIfNull())
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

                    // Add video to the list if it's not already there
                    if (videoIds.Add(videoId))
                    {
                        videos.Add(new Video(videoId, videoAuthor, videoUploadDate, videoTitle, videoDescription,
                            videoThumbnails, videoDuration, videoKeywords, videoStatistics));

                        // Increment delta
                        countDelta++;
                    }
                }

                // If no distinct videos were added to the list - break
                if (countDelta <= 0)
                    break;

                // Advance index and page
                index += 100;
                page++;
            } while (page <= maxPages);

            // Extract playlist info
            var author = playlistJson.SelectToken("author")?.Value<string>() ?? ""; // system playlists have no author
            var title = playlistJson.SelectToken("title").Value<string>();
            var description = playlistJson.SelectToken("description")?.Value<string>() ?? "";
            var viewCount = playlistJson.SelectToken("views")?.Value<long>() ?? 0; // system playlists have no views
            var likeCount = playlistJson.SelectToken("likes")?.Value<long>() ?? 0; // system playlists have no likes
            var dislikeCount = playlistJson.SelectToken("dislikes")?.Value<long>() ?? 0; // system playlists have no dislikes

            // Create statistics
            var statistics = new Statistics(viewCount, likeCount, dislikeCount);

            return new Playlist(playlistId, author, title, description, statistics, videos);
        }

        /// <inheritdoc />
        public Task<Playlist> GetPlaylistAsync(string playlistId) => GetPlaylistAsync(playlistId, int.MaxValue);
    }
}