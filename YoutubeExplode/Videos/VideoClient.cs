using System;
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
        public StreamsClient Streams { get; }

        /// <summary>
        /// Queries related to closed captions of YouTube videos.
        /// </summary>
        public ClosedCaptionTrackClient ClosedCaptionTracks { get; }

        /// <summary>
        /// Initializes an instance of <see cref="VideoClient"/>.
        /// </summary>
        public VideoClient(HttpClient httpClient)
        {
            _httpClient = httpClient;

            Streams = new StreamsClient(httpClient);
            ClosedCaptionTracks = new ClosedCaptionTrackClient(httpClient);
        }

        /// <summary>
        /// Gets the metadata associated with the specified video.
        /// </summary>
        public async Task<Video> GetAsync(VideoId id)
        {
            var videoInfoResponse = await VideoInfoResponse.GetAsync(_httpClient, id);
            var playerResponse = videoInfoResponse.GetPlayerResponse();

            var watchPage = await WatchPage.GetAsync(_httpClient, id);

            return new Video(
                id,
                playerResponse.GetVideoTitle(),
                playerResponse.GetVideoAuthor(),
                watchPage.GetVideoUploadDate(),
                playerResponse.GetVideoDescription(),
                playerResponse.GetVideoDuration(),
                Array.Empty<Thumbnail>(),
                playerResponse.GetVideoKeywords(),
                new Engagement(
                    playerResponse.TryGetVideoViewCount() ?? 0,
                    watchPage.TryGetVideoLikeCount() ?? 0,
                    watchPage.TryGetVideoDislikeCount() ?? 0
                )
            );
        }
    }
}