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

            // Get playlist info parser for the first page
            var playlistInfoParser = await GetPlaylistInfoParserAsync(playlistId, 0);

            // Extract info
            var author = playlistInfoParser.TryGetAuthor() ?? "";
            var title = playlistInfoParser.GetTitle();
            var description = playlistInfoParser.TryGetDescription() ?? "";
            var viewCount = playlistInfoParser.TryGetViewCount() ?? 0;
            var likeCount = playlistInfoParser.TryGetLikeCount() ?? 0;
            var dislikeCount = playlistInfoParser.TryGetDislikeCount() ?? 0;

            // Process videos from all pages
            var page = 0;
            var index = 0;
            var videoIds = new HashSet<string>();
            var videos = new List<Video>();
            do
            {
                var countTotal = 0;
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

                // Get playlist info parser for the next page
                playlistInfoParser = await GetPlaylistInfoParserAsync(playlistId, index);
            } while (page < maxPages);

            var statistics = new Statistics(viewCount, likeCount, dislikeCount);

            return new Playlist(playlistId, author, title, description, statistics, videos);
        }

        /// <inheritdoc />
        public Task<Playlist> GetPlaylistAsync(string playlistId) => GetPlaylistAsync(playlistId, int.MaxValue);
    }
}