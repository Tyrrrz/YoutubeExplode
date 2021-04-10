using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using YoutubeExplode.Bridge;
using YoutubeExplode.Common;
using YoutubeExplode.Exceptions;
using YoutubeExplode.Videos.ClosedCaptions;
using YoutubeExplode.Videos.Streams;

namespace YoutubeExplode.Videos
{
    /// <summary>
    /// Queries related to YouTube videos.
    /// </summary>
    public class VideoClient
    {
        private readonly YoutubeController _youtubeController;

        /// <summary>
        /// Queries related to media streams of YouTube videos.
        /// </summary>
        public StreamClient Streams { get; }

        /// <summary>
        /// Queries related to closed captions of YouTube videos.
        /// </summary>
        public ClosedCaptionClient ClosedCaptions { get; }

        /// <summary>
        /// Initializes an instance of <see cref="VideoClient"/>.
        /// </summary>
        public VideoClient(HttpClient httpClient)
        {
            _youtubeController = new YoutubeController(httpClient);

            Streams = new StreamClient(httpClient);
            ClosedCaptions = new ClosedCaptionClient(httpClient);
        }

        /// <summary>
        /// Gets the metadata associated with the specified video.
        /// </summary>
        public async ValueTask<Video> GetAsync(
            VideoId videoId,
            CancellationToken cancellationToken = default)
        {
            var watchPage = await _youtubeController.GetVideoWatchPageAsync(videoId, cancellationToken);

            var playerResponse =
                watchPage.TryGetPlayerResponse() ??
                throw new YoutubeExplodeException("Could not extract player response.");

            var title =
                playerResponse.TryGetVideoTitle() ??
                throw new YoutubeExplodeException("Could not extract video title.");

            var author =
                playerResponse.TryGetVideoAuthor() ??
                throw new YoutubeExplodeException("Could not extract video author.");

            var channelId =
                playerResponse.TryGetVideoChannelId() ??
                throw new YoutubeExplodeException("Could not extract video channel ID.");

            var uploadDate =
                playerResponse.TryGetVideoUploadDate() ??
                throw new YoutubeExplodeException("Could not extract video upload date.");

            var description =
                playerResponse.TryGetVideoDescription() ??
                throw new YoutubeExplodeException("Could not extract video description.");

            var duration =
                playerResponse.TryGetVideoDuration() ??
                throw new YoutubeExplodeException("Could not extract video duration.");

            var viewCount =
                playerResponse.TryGetVideoViewCount() ??
                throw new YoutubeExplodeException("Could not extract video view count.");

            var likeCount =
                watchPage.TryGetVideoLikeCount() ??
                0; // like count is inaccessible if likes are disabled

            var dislikeCount =
                watchPage.TryGetVideoDislikeCount() ??
                0; // dislike count is inaccessible if likes are disabled

            return new Video(
                videoId,
                title,
                author,
                channelId,
                uploadDate,
                description,
                duration,
                Array.Empty<Thumbnail>(), // todo
                playerResponse.GetVideoKeywords(),
                new Engagement(
                    viewCount,
                    likeCount,
                    dislikeCount
                )
            );
        }
    }
}