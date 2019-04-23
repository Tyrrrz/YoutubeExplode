using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using YoutubeExplode.Exceptions;
using YoutubeExplode.Internal;
using YoutubeExplode.Internal.Decoders;

namespace YoutubeExplode
{
    /// <summary>
    /// The entry point for <see cref="YoutubeExplode"/>.
    /// </summary>
    public partial class YoutubeClient : IYoutubeClient
    {
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Creates an instance of <see cref="YoutubeClient"/>.
        /// </summary>
        public YoutubeClient(HttpClient httpClient)
        {
            _httpClient = httpClient.GuardNotNull(nameof(httpClient));
        }

        /// <summary>
        /// Creates an instance of <see cref="YoutubeClient"/>.
        /// </summary>
        public YoutubeClient()
            : this(HttpClientEx.GetSingleton())
        {
        }

        private async Task<VideoEmbedPageDecoder> GetVideoEmbedPageDecoderAsync(string videoId)
        {
            var url = $"https://www.youtube.com/embed/{videoId}?disable_polymer=true&hl=en";
            var raw = await _httpClient.GetStringAsync(url);

            return VideoEmbedPageDecoder.Initialize(raw);
        }

        private async Task<VideoWatchPageDecoder> GetVideoWatchPageDecoderAsync(string videoId)
        {
            var url = $"https://www.youtube.com/watch?v={videoId}&disable_polymer=true&bpctr=9999999999&hl=en";
            var raw = await _httpClient.GetStringAsync(url);

            return VideoWatchPageDecoder.Initialize(raw);
        }

        private async Task<VideoInfoDecoder> GetVideoInfoDecoderAsync(string videoId, string sts = null)
        {
            // This parameter does magic and a lot of videos don't work without it
            var eurl = $"https://youtube.googleapis.com/v/{videoId}".UrlEncode();

            // Execute request
            var url = $"https://www.youtube.com/get_video_info?video_id={videoId}&el=embedded&sts={sts}&eurl={eurl}&hl=en";
            var raw = await _httpClient.GetStringAsync(url);

            // Initialize
            var decoder = VideoInfoDecoder.Initialize(raw);

            // If invalid - throw
            if (!decoder.Validate())
                throw new VideoUnavailableException(videoId, $"Video [{videoId}] is unavailable.");

            return decoder;
        }

        private async Task<PlayerSourceDecoder> GetPlayerSourceDecoderAsync(string sourceUrl)
        {
            var raw = await _httpClient.GetStringAsync(sourceUrl);
            return PlayerSourceDecoder.Initialize(raw);
        }

        private async Task<DashManifestDecoder> GetDashManifestDecoderAsync(string dashManifestUrl)
        {
            var raw = await _httpClient.GetStringAsync(dashManifestUrl);
            return DashManifestDecoder.Initialize(raw);
        }

        private async Task<ClosedCaptionTrackDecoder> GetClosedCaptionTrackDecoderAsync(string url)
        {
            var raw = await _httpClient.GetStringAsync(url);
            return ClosedCaptionTrackDecoder.Initialize(raw);
        }

        private async Task<ChannelPageDecoder> GetChannelPageDecoderAsync(string channelId)
        {
            var url = $"https://www.youtube.com/channel/{channelId}?hl=en";

            // Retry up to 5 times because sometimes the response has random errors
            for (var retry = 0; retry < 5; retry++)
            {
                var raw = await _httpClient.GetStringAsync(url);
                var decoder = ChannelPageDecoder.Initialize(raw);

                // If successful - return
                if (decoder.Validate())
                    return decoder;

                // Otherwise put a delay before trying again
                await Task.Delay(150);
            }

            // Throw exception
            throw new InvalidDataException("Could not get channel page.");
        }

        private async Task<ChannelPageDecoder> GetChannelPageDecoderForUserAsync(string username)
        {
            username = username.UrlEncode();

            var url = $"https://www.youtube.com/user/{username}?hl=en";

            // Retry up to 5 times because sometimes the response has random errors
            for (var retry = 0; retry < 5; retry++)
            {
                var raw = await _httpClient.GetStringAsync(url);
                var decoder = ChannelPageDecoder.Initialize(raw);

                // If successful - return
                if (decoder.Validate())
                    return decoder;

                // Otherwise put a delay before trying again
                await Task.Delay(150);
            }

            // Throw exception
            throw new InvalidDataException("Could not get channel page.");
        }

        private async Task<PlaylistInfoDecoder> GetPlaylistInfoDecoderAsync(string playlistId, int index)
        {
            var url = $"https://www.youtube.com/list_ajax?style=json&action_get_list=1&list={playlistId}&index={index}&hl=en";
            var raw = await _httpClient.GetStringAsync(url);

            return PlaylistInfoDecoder.Initialize(raw);
        }

        private async Task<PlaylistInfoDecoder> GetPlaylistInfoDecoderForSearchAsync(string query, int page)
        {
            query = query.UrlEncode();

            // Don't ensure success here so that empty pages can be parsed

            var url = $"https://www.youtube.com/search_ajax?style=json&search_query={query}&page={page}&hl=en";
            var raw = await _httpClient.GetStringAsync(url, false);

            return PlaylistInfoDecoder.Initialize(raw);
        }
    }
}