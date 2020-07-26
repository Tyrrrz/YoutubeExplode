using System.Threading.Tasks;
using YoutubeExplode.Common;
using YoutubeExplode.ReverseEngineering;
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
        private readonly YoutubeHttpClient _httpClient;

        /// <summary>
        /// Queries related to media streams of YouTube videos.
        /// </summary>
        public StreamsClient Streams { get; }

        /// <summary>
        /// Queries related to closed captions of YouTube videos.
        /// </summary>
        public ClosedCaptionClient ClosedCaptions { get; }

        /// <summary>
        /// Initializes an instance of <see cref="VideoClient"/>.
        /// </summary>
        internal VideoClient(YoutubeHttpClient httpClient)
        {
            _httpClient = httpClient;

            Streams = new StreamsClient(httpClient);
            ClosedCaptions = new ClosedCaptionClient(httpClient);
        }

        /// <summary>
        /// Gets the metadata associated with the specified video.
        /// </summary>
        /// <param name="id">The video id</param>
        /// <param name="fastMode">If enabled, this method is up to 4x faster, but doesnt include like/dislike count and disables caching</param>
        public async Task<Video> GetAsync(VideoId id, bool fastMode = false)
        {
            var videoInfoResponse = await VideoInfoResponse.GetAsync(_httpClient, id);
            var playerResponse = videoInfoResponse.GetPlayerResponse();

            if (!fastMode)
            {
                var watchPage = await WatchPage.GetAsync(_httpClient, id);

                return new Video(
                    id,
                    playerResponse.GetVideoTitle(),
                    playerResponse.GetVideoAuthor(),
                    playerResponse.GetVideoChannelId(),
                    playerResponse.GetVideoUploadDate(),
                    playerResponse.GetVideoDescription(),
                    playerResponse.GetVideoDuration(),
                    new ThumbnailSet(id),
                    playerResponse.GetVideoKeywords(),
                    new Engagement(
                        playerResponse.TryGetVideoViewCount() ?? 0,
                        watchPage.TryGetVideoLikeCount() ?? 0,
                        watchPage.TryGetVideoDislikeCount() ?? 0
                    )
                ) { 
                    PlayerConfig = watchPage.TryGetPlayerConfig() ?? null
                };
            }
            else
            {
                return new Video(
                    id,
                    playerResponse.GetVideoTitle(),
                    playerResponse.GetVideoAuthor(),
                    playerResponse.GetVideoChannelId(),
                    playerResponse.GetVideoUploadDate(),
                    playerResponse.GetVideoDescription(),
                    playerResponse.GetVideoDuration(),
                    new ThumbnailSet(id),
                    playerResponse.GetVideoKeywords(),
                    new Engagement(
                        playerResponse.TryGetVideoViewCount() ?? 0,
                        0,
                        0
                    )
                );
            }
        }
    }
}