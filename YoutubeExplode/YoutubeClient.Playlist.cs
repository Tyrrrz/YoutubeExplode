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

            // Get playlist info decoder for the first page
            var playlistInfoDecoder = await GetPlaylistInfoDecoderAsync(playlistId, 0);

            // Extract info
            var author = playlistInfoDecoder.TryGetAuthor() ?? "";
            var title = playlistInfoDecoder.GetTitle();
            var description = playlistInfoDecoder.TryGetDescription() ?? "";
            var viewCount = playlistInfoDecoder.TryGetViewCount() ?? 0;
            var likeCount = playlistInfoDecoder.TryGetLikeCount() ?? 0;
            var dislikeCount = playlistInfoDecoder.TryGetDislikeCount() ?? 0;

            // Process videos from all pages
            var page = 0;
            var index = 0;
            var videoIds = new HashSet<string>();
            var videos = new List<Video>();
            do
            {
                var countTotal = 0;
                var countDelta = 0;
                foreach (var videoInfoDecoder in playlistInfoDecoder.GetVideos())
                {
                    // Extract info
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

                // Get playlist info decoder for the next page
                playlistInfoDecoder = await GetPlaylistInfoDecoderAsync(playlistId, index);
            } while (page < maxPages);

            var statistics = new Statistics(viewCount, likeCount, dislikeCount);

            return new Playlist(playlistId, author, title, description, statistics, videos);
        }

        /// <inheritdoc />
        public Task<Playlist> GetPlaylistAsync(string playlistId) => GetPlaylistAsync(playlistId, int.MaxValue);
    }
}