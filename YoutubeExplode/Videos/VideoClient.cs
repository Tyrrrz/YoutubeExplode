using System.Net.Http;
using System.Threading.Tasks;
using YoutubeExplode.Common;
using YoutubeExplode.ReverseEngineering.Responses;
using YoutubeExplode.Videos.ClosedCaptions;
using YoutubeExplode.Videos.Streams;

namespace YoutubeExplode.Videos
{
    /// <summary>
    /// Queries related to YouTube videos.
    /// </summary>
    public class VideoClient
    {
        private readonly HttpClient _httpClient;

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
            _httpClient = httpClient;

            Streams = new StreamClient(httpClient);
            ClosedCaptions = new ClosedCaptionClient(httpClient);
        }

        private async ValueTask<Video> GetVideoFromWatchPageAsync(VideoId videoId)
        {
            var videoInfoResponse = await VideoInfoResponse.GetAsync(_httpClient, videoId);
            var playerResponse = videoInfoResponse.GetPlayerResponse();

            var watchPage = await WatchPage.GetAsync(_httpClient, videoId);

            return new Video(
                videoId,
                playerResponse.GetVideoTitle(),
                playerResponse.GetVideoAuthor(),
                playerResponse.GetVideoChannelId(),
                playerResponse.GetVideoUploadDate(),
                playerResponse.GetVideoDescription(),
                playerResponse.GetVideoDuration(),
                new ThumbnailSet(videoId),
                playerResponse.GetVideoKeywords(),
                new Engagement(
                    playerResponse.TryGetVideoViewCount() ?? 0,
                    watchPage.TryGetVideoLikeCount() ?? 0,
                    watchPage.TryGetVideoDislikeCount() ?? 0
                )
            );
        }

        /// <summary>
        /// Gets the metadata associated with the specified video.
        /// </summary>
        public async ValueTask<Video> GetAsync(VideoId id)
        {
            return await GetVideoFromWatchPageAsync(id);
        }
    }
}
