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
                // Get playlist info parser
                var playlistInfoParser = await GetPlaylistInfoParserForSearchAsync(query, page);

                // Extract videos
                var countDelta = 0;
                foreach (var videoInfoParser in playlistInfoParser.GetVideos())
                {
                    // Extract info
                    var videoId = videoInfoParser.GetVideoId();
                    var videoAuthor = videoInfoParser.GetVideoAuthor();
                    var videoUploadDate = videoInfoParser.GetVideoUploadDate();
                    var videoTitle = videoInfoParser.GetVideoTitle();
                    var videoDescription = videoInfoParser.GetVideoDescription();
                    var videoDuration = videoInfoParser.GetVideoDuration();
                    var videoKeywords = videoInfoParser.GetVideoKeywords();
                    var videoViewCount = videoInfoParser.GetVideoViewCount();
                    var videoLikeCount = videoInfoParser.GetVideoLikeCount();
                    var videoDislikeCount = videoInfoParser.GetVideoDislikeCount();

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