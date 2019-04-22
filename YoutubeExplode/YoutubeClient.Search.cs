using System.Collections.Generic;
using System.Threading.Tasks;
using YoutubeExplode.Internal;
using YoutubeExplode.Models;

namespace YoutubeExplode
{
    public partial class YoutubeClient
    {
        /// <inheritdoc />
        public async Task<IReadOnlyList<Video>> SearchVideosAsync(string query, int maxPages)
        {
            query.GuardNotNull(nameof(query));
            maxPages.GuardPositive(nameof(maxPages));

            // Get all videos across pages
            var videos = new List<Video>();
            for (var page = 1; page <= maxPages; page++)
            {
                // Get search playlist AJAX
                var playlistAjax = await GetSearchPlaylistAjaxAsync(query, page);

                // Parse videos
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

                    videos.Add(new Video(videoId, videoAuthor, videoUploadDate, videoTitle, videoDescription,
                        videoThumbnails, videoDuration, videoKeywords, videoStatistics));

                    countDelta++;
                }

                // Break if no distinct videos were added to the list
                if (countDelta <= 0)
                    break;
            }

            return videos;
        }

        /// <inheritdoc />
        public Task<IReadOnlyList<Video>> SearchVideosAsync(string query) => SearchVideosAsync(query, int.MaxValue);
    }
}