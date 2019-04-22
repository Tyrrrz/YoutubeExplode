using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using YoutubeExplode.Exceptions;
using YoutubeExplode.Internal;
using YoutubeExplode.Internal.Abstractions.Wrappers;

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

        private async Task<VideoEmbedPage> GetVideoEmbedPageAsync(string videoId)
        {
            var url = $"https://www.youtube.com/embed/{videoId}?disable_polymer=true&hl=en";
            var raw = await _httpClient.GetStringAsync(url);

            return VideoEmbedPage.Initialize(raw);
        }

        private async Task<VideoWatchPage> GetVideoWatchPageAsync(string videoId)
        {
            var url = $"https://www.youtube.com/watch?v={videoId}&disable_polymer=true&bpctr=9999999999&hl=en";
            var raw = await _httpClient.GetStringAsync(url);

            return VideoWatchPage.Initialize(raw);
        }

        private async Task<VideoAjax> GetVideoAjaxAsync(string videoId, string sts = null)
        {
            // This parameter does magic and a lot of videos don't work without it
            var eurl = $"https://youtube.googleapis.com/v/{videoId}".UrlEncode();

            // Execute request
            var url = $"https://www.youtube.com/get_video_info?video_id={videoId}&el=embedded&sts={sts}&eurl={eurl}&hl=en";
            var raw = await _httpClient.GetStringAsync(url);

            // Initialize parser
            var parser = VideoAjax.Initialize(raw);

            // If the video is unavailable - throw
            if (!parser.Validate())
                throw new VideoUnavailableException(videoId, $"Video [{videoId}] is unavailable.");

            return parser;
        }

        private async Task<PlayerSource> GetPlayerSourceAsync(string sourceUrl)
        {
            var raw = await _httpClient.GetStringAsync(sourceUrl);
            return PlayerSource.Initialize(raw);
        }

        private async Task<DashManifest> GetDashManifestAsync(string dashManifestUrl)
        {
            var raw = await _httpClient.GetStringAsync(dashManifestUrl);
            return DashManifest.Initialize(raw);
        }

        private async Task<ClosedCaptionTrackAjax> GetClosedCaptionTrackAjaxAsync(string url)
        {
            var raw = await _httpClient.GetStringAsync(url);
            return ClosedCaptionTrackAjax.Initialize(raw);
        }

        private async Task<ChannelPage> GetChannelPageAsync(string channelId)
        {
            var url = $"https://www.youtube.com/channel/{channelId}?hl=en";

            // Retry up to 5 times because sometimes the response has random errors
            for (var retry = 0; retry < 5; retry++)
            {
                var raw = await _httpClient.GetStringAsync(url);
                var parser = ChannelPage.Initialize(raw);

                // If successful - return
                if (parser.Validate())
                    return parser;

                // Otherwise put a delay before trying again
                await Task.Delay(150);
            }

            // Throw exception
            throw new InvalidDataException("Could not get channel page.");
        }

        private async Task<ChannelPage> GetUserChannelPageAsync(string username)
        {
            username = username.UrlEncode();

            var url = $"https://www.youtube.com/user/{username}?hl=en";

            // Retry up to 5 times because sometimes the response has random errors
            for (var retry = 0; retry < 5; retry++)
            {
                var raw = await _httpClient.GetStringAsync(url);
                var parser = ChannelPage.Initialize(raw);

                // If successful - return
                if (parser.Validate())
                    return parser;

                // Otherwise put a delay before trying again
                await Task.Delay(150);
            }

            // Throw exception
            throw new InvalidDataException("Could not get channel page.");
        }

        private async Task<PlaylistAjax> GetPlaylistAjaxAsync(string playlistId, int index)
        {
            var url = $"https://www.youtube.com/list_ajax?style=json&action_get_list=1&list={playlistId}&index={index}&hl=en";
            var raw = await _httpClient.GetStringAsync(url);

            return PlaylistAjax.Initialize(raw);
        }

        private async Task<PlaylistAjax> GetSearchPlaylistAjaxAsync(string query, int page)
        {
            query = query.UrlEncode();

            // Don't ensure success here so that empty pages could be parsed

            var url = $"https://www.youtube.com/search_ajax?style=json&search_query={query}&page={page}&hl=en";
            var raw = await _httpClient.GetStringAsync(url, false);

            return PlaylistAjax.Initialize(raw);
        }
    }
}