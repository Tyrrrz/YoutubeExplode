using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using YoutubeExplode.Internal;
using YoutubeExplode.Internal.Parsers;
using YoutubeExplode.Models;

namespace YoutubeExplode
{
    public partial class YoutubeClient
    {
        private async Task<PlaylistAjaxParser> GetPlaylistAjaxParserAsync(string playlistId, int index)
        {
            var url =
                $"https://www.youtube.com/list_ajax?style=json&action_get_list=1&list={playlistId}&index={index}&hl=en";
            var raw = await _httpClient.GetStringAsync(url).ConfigureAwait(false);

            return PlaylistAjaxParser.Initialize(raw);
        }

        /// <inheritdoc />
        public async Task<Playlist> GetPlaylistAsync(string playlistId, int maxPages)
        {
            playlistId.GuardNotNull(nameof(playlistId));
            maxPages.GuardPositive(nameof(maxPages));

            if (!ValidatePlaylistId(playlistId))
                throw new ArgumentException($"Invalid YouTube playlist ID [{playlistId}].", nameof(playlistId));

            // Get playlist parser for the first page
            var parser = await GetPlaylistAjaxParserAsync(playlistId, 0).ConfigureAwait(false);

            // Extract info
            var title = parser.GetTitle();
            var author = parser.GetAuthor();
            var description = parser.GetDescription();
            var viewCount = parser.GetViewCount();
            var likeCount = parser.GetLikeCount();
            var dislikeCount = parser.GetDislikeCount();

            // Extract collective video info from all pages
            var pagesDone = 0;
            var index = 0;
            var videoIds = new HashSet<string>();
            var videos = new List<Video>();
            do
            {
                // Parse videos
                var total = 0;
                var delta = 0;
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
                index += total;

                // Get parser for next page
                parser = await GetPlaylistAjaxParserAsync(playlistId, index).ConfigureAwait(false);
            } while (pagesDone < maxPages);

            var statistics = new Statistics(viewCount, likeCount, dislikeCount);
            return new Playlist(playlistId, author, title, description, statistics, videos);
        }

        /// <inheritdoc />
        public Task<Playlist> GetPlaylistAsync(string playlistId)
            => GetPlaylistAsync(playlistId, int.MaxValue);
    }
}