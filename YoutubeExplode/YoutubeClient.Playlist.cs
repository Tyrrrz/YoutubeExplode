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
        private async Task<string> GetPlaylistInfoRawAsync(string playlistId, int index = 0)
        {
            var url =
                $"https://www.youtube.com/list_ajax?style=json&action_get_list=1&list={playlistId}&index={index}&hl=en";
            return await _httpClient.GetStringAsync(url).ConfigureAwait(false);
        }

        private async Task<JToken> GetPlaylistInfoAsync(string playlistId, int index = 0)
        {
            var raw = await GetPlaylistInfoRawAsync(playlistId, index).ConfigureAwait(false);
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
            var pagesDone = 0;
            var offset = 0;
            JToken playlistJson;
            var videoIds = new HashSet<string>();
            var videos = new List<Video>();
            do
            {
                // Get playlist info
                playlistJson = await GetPlaylistInfoAsync(playlistId, offset).ConfigureAwait(false);

                // Parse videos
                var total = 0;
                var delta = 0;
                foreach (var videoJson in playlistJson["video"])
                {
                    // Basic info
                    var videoId = videoJson["encrypted_id"].Value<string>();
                    var videoAuthor = videoJson["author"].Value<string>();
                    var videoUploadDate = videoJson["added"].Value<string>().ParseDateTimeOffset("M/d/yy");
                    var videoTitle = videoJson["title"].Value<string>();
                    var videoDuration = TimeSpan.FromSeconds(videoJson["length_seconds"].Value<double>());
                    var videoDescription = videoJson["description"].Value<string>();

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

                    // Add to list if not already there
                    if (videoIds.Add(video.Id))
                    {
                        videos.Add(video);
                        delta++;
                    }
                    total++;
                }

                // Break if no distinct videos were added to the list
                if (delta <= 0)
                    break;

                // Prepare for next page
                pagesDone++;
                offset += total;
            } while (pagesDone < maxPages);

            // Extract playlist info
            var title = playlistJson["title"].Value<string>();
            var author = playlistJson["author"]?.Value<string>() ?? ""; // system playlists don't have an author
            var description = playlistJson["description"]?.Value<string>() ?? ""; // system playlists don't have description

            // Statistics
            var viewCount = playlistJson["views"]?.Value<long>() ?? 0; // watchlater does not have views
            var likeCount = playlistJson["likes"]?.Value<long>() ?? 0; // system playlists don't have likes
            var dislikeCount = playlistJson["dislikes"]?.Value<long>() ?? 0; // system playlists don't have dislikes
            var statistics = new Statistics(viewCount, likeCount, dislikeCount);

            return new Playlist(playlistId, author, title, description, statistics, videos);
        }

        /// <inheritdoc />
        public Task<Playlist> GetPlaylistAsync(string playlistId)
            => GetPlaylistAsync(playlistId, int.MaxValue);
    }
}