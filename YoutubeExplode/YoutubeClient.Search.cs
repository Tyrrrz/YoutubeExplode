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
                // Get playlist info decoder
                var playlistInfoDecoder = await GetPlaylistInfoDecoderForSearchAsync(query, page);

                // Parse videos
                var countDelta = 0;
                foreach (var videoInfoDecoder in playlistInfoDecoder.GetVideos())
                {
                    // Parse info
                    var videoId = videoInfoDecoder.GetVideoId();
                    var videoAuthor = videoInfoDecoder.GetVideoAuthor();
                    var videoUploadDate = videoInfoDecoder.GetVideoUploadDate();
                    var videoTitle = videoInfoDecoder.GetVideoTitle();
                    var videoDescription = videoInfoDecoder.GetVideoDescription();
                    var videoDuration = videoInfoDecoder.GetVideoDuration();
                    var videoKeywords = videoInfoDecoder.GetVideoKeywords();
                    var videoViewCount = videoInfoDecoder.GetVideoViewCount();
                    var videoLikeCount = videoInfoDecoder.GetVideoLikeCount();
                    var videoDislikeCount = videoInfoDecoder.GetVideoDislikeCount();

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