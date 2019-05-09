using System.Net.Http;
using System.Threading.Tasks;
using YoutubeExplode.Exceptions;
using YoutubeExplode.Internal;
using YoutubeExplode.Internal.Parsers;

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

        private async Task<VideoEmbedPageParser> GetVideoEmbedPageParserAsync(string videoId)
        {
            // Execute request
            var url = $"https://www.youtube.com/embed/{videoId}?disable_polymer=true&hl=en";
            var raw = await _httpClient.GetStringAsync(url);

            // Initialize parser
            return VideoEmbedPageParser.Initialize(raw);
        }

        private async Task<VideoWatchPageParser> GetVideoWatchPageParserAsync(string videoId)
        {
            // Execute request
            var url = $"https://www.youtube.com/watch?v={videoId}&disable_polymer=true&bpctr=9999999999&hl=en";
            var raw = await _httpClient.GetStringAsync(url);

            // Initialize parser
            return VideoWatchPageParser.Initialize(raw);
        }

        private async Task<VideoInfoParser> GetVideoInfoParserAsync(string videoId, string sts = null)
        {
            // This parameter does magic and a lot of videos don't work without it
            var eurl = $"https://youtube.googleapis.com/v/{videoId}".UrlEncode();

            // Execute request
            var url = $"https://www.youtube.com/get_video_info?video_id={videoId}&el=embedded&sts={sts}&eurl={eurl}&hl=en";
            var raw = await _httpClient.GetStringAsync(url);

            // Initialize parser
            var parser = VideoInfoParser.Initialize(raw);

            // If video ID is not set - video is unavailable
            if (parser.GetVideoId().IsNullOrWhiteSpace())
                throw new VideoUnavailableException(videoId, $"Video [{videoId}] is unavailable.");

            return parser;
        }

        private async Task<PlayerSourceParser> GetPlayerSourceParserAsync(string sourceUrl)
        {
            // Execute request
            var raw = await _httpClient.GetStringAsync(sourceUrl);

            // Initialize parser
            return PlayerSourceParser.Initialize(raw);
        }

        private async Task<DashManifestParser> GetDashManifestParserAsync(string dashManifestUrl)
        {
            // Execute request
            var raw = await _httpClient.GetStringAsync(dashManifestUrl);

            // Initialize parser
            return DashManifestParser.Initialize(raw);
        }

        private async Task<ChannelPageParser> GetChannelPageParserAsync(string channelId)
        {
            var url = $"https://www.youtube.com/channel/{channelId}?hl=en";

            // Retry up to 5 times because sometimes the response has random errors
            for (var retry = 0; retry < 5; retry++)
            {
                // Execute request
                var raw = await _httpClient.GetStringAsync(url);

                // Initialize parser
                var parser = ChannelPageParser.Initialize(raw);

                // If successful - return
                if (parser.Validate())
                    return parser;

                // Otherwise put a delay before trying again
                await Task.Delay(150);
            }

            // Throw exception
            throw new ParserException("Could not get channel page.");
        }

        private async Task<ChannelPageParser> GetChannelPageParserForUserAsync(string username)
        {
            username = username.UrlEncode();

            var url = $"https://www.youtube.com/user/{username}?hl=en";

            // Retry up to 5 times because sometimes the response has random errors
            for (var retry = 0; retry < 5; retry++)
            {
                // Execute request
                var raw = await _httpClient.GetStringAsync(url);

                // Initialize parser
                var parser = ChannelPageParser.Initialize(raw);

                // If successful - return
                if (parser.Validate())
                    return parser;

                // Otherwise put a delay before trying again
                await Task.Delay(150);
            }

            // Throw exception
            throw new ParserException("Could not get channel page.");
        }
    }
}