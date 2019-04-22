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

            // Get playlist AJAX for the first page
            var playlistAjax = await GetPlaylistAjaxAsync(playlistId, 0);

            // Parse info
            var author = playlistAjax.TryGetAuthor() ?? "";
            var title = playlistAjax.GetTitle();
            var description = playlistAjax.TryGetDescription() ?? "";
            var viewCount = playlistAjax.TryGetViewCount() ?? 0;
            var likeCount = playlistAjax.TryGetLikeCount() ?? 0;
            var dislikeCount = playlistAjax.TryGetDislikeCount() ?? 0;

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
                foreach (var videoParser in playlistAjax.GetVideos())
                {
                    // Parse info
                    var videoId = videoParser.GetVideoId();
                    var videoAuthor = videoParser.GetVideoAuthor();
                    var videoUploadDate = videoParser.GetVideoUploadDate();
                    var videoTitle = videoParser.GetVideoTitle();
                    var videoDescription = videoParser.GetVideoDescription();
                    var videoDuration = videoParser.GetVideoDuration();
                    var videoKeywords = videoParser.GetVideoKeywords();
                    var videoViewCount = videoParser.GetVideoViewCount();
                    var videoLikeCount = videoParser.GetVideoLikeCount();
                    var videoDislikeCount = videoParser.GetVideoDislikeCount();

                    var videoStatistics = new Statistics(videoViewCount, videoLikeCount, videoDislikeCount);
                    var videoThumbnails = new ThumbnailSet(videoId);

                    // Add video to the list if it's not already there
                    if (videoIds.Add(videoId))
                    {
                        videos.Add(new Video(videoId, videoAuthor, videoUploadDate, videoTitle, videoDescription,
                            videoThumbnails, videoDuration, videoKeywords, videoStatistics));

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

                // Get playlist AJAX for the next page
                playlistAjax = await GetPlaylistAjaxAsync(playlistId, index);
            } while (page < maxPages);

            var statistics = new Statistics(viewCount, likeCount, dislikeCount);

            return new Playlist(playlistId, author, title, description, statistics, videos);
        }

        /// <inheritdoc />
        public Task<Playlist> GetPlaylistAsync(string playlistId) => GetPlaylistAsync(playlistId, int.MaxValue);
    }
}