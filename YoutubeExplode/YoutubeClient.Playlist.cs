using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using YoutubeExplode.Internal;
using YoutubeExplode.Models;

namespace YoutubeExplode
{
    public partial class YoutubeClient
    {
        /// <inheritdoc />
        public async Task<Playlist> GetPlaylistAsync(string playlistId, int maxPages)
        {
            playlistId.GuardNotNull(nameof(playlistId));
            maxPages.GuardPositive(nameof(maxPages));

            if (!ValidatePlaylistId(playlistId))
                throw new ArgumentException($"Invalid YouTube playlist ID [{playlistId}].", nameof(playlistId));

            // Get parser for the first page
            var parser = await GetPlaylistAjaxParserAsync(playlistId, 0).ConfigureAwait(false);

            // Parse info
            var author = parser.ParseAuthor();
            var title = parser.ParseTitle();
            var description = parser.ParseDescription();
            var viewCount = parser.ParseViewCount();
            var likeCount = parser.ParseLikeCount();
            var dislikeCount = parser.ParseDislikeCount();

            // Parse videos from all pages
            var page = 0;
            var index = 0;
            var videoIds = new HashSet<string>();
            var videos = new List<Video>();
            do
            {
                // Parse videos
                var countTotal = 0;
                var countDelta = 0;
                foreach (var videoParser in parser.GetVideos())
                {
                    // Parse info
                    var videoId = videoParser.ParseId();
                    var videoAuthor = videoParser.ParseAuthor();
                    var videoUploadDate = videoParser.ParseUploadDate();
                    var videoTitle = videoParser.ParseTitle();
                    var videoDescription = videoParser.ParseDescription();
                    var videoDuration = videoParser.ParseDuration();
                    var videoKeywords = videoParser.ParseKeywords();
                    var videoViewCount = videoParser.ParseViewCount();
                    var videoLikeCount = videoParser.ParseLikeCount();
                    var videoDislikeCount = videoParser.ParseDislikeCount();

                    var videoStatistics = new Statistics(videoViewCount, videoLikeCount, videoDislikeCount);
                    var videoThumbnails = new ThumbnailSet(videoId);

                    var video = new Video(videoId, videoAuthor, videoUploadDate, videoTitle, videoDescription,
                        videoThumbnails, videoDuration, videoKeywords, videoStatistics);

                    // Add video to the list if it's not already there
                    if (videoIds.Add(video.Id))
                    {
                        videos.Add(video);
                        countDelta++;
                    }

                    countTotal++;
                }

                // Break if no distinct videos were added to the list
                if (countDelta <= 0)
                    break;

                // Prepare for the next page
                page++;
                index += countTotal;

                // Get parser for the next page
                parser = await GetPlaylistAjaxParserAsync(playlistId, index).ConfigureAwait(false);
            } while (page < maxPages);

            var statistics = new Statistics(viewCount, likeCount, dislikeCount);

            return new Playlist(playlistId, author, title, description, statistics, videos);
        }

        /// <inheritdoc />
        public Task<Playlist> GetPlaylistAsync(string playlistId)
            => GetPlaylistAsync(playlistId, int.MaxValue);
    }
}