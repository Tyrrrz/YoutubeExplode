using System.IO;
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
            var url = $"https://www.youtube.com/embed/{videoId}?disable_polymer=true&hl=en";
            var raw = await _httpClient.GetStringAsync(url);

            return VideoEmbedPageParser.Initialize(raw);
        }

        private async Task<VideoWatchPageParser> GetVideoWatchPageParserAsync(string videoId)
        {
            var url = $"https://www.youtube.com/watch?v={videoId}&disable_polymer=true&bpctr=9999999999&hl=en";
            var raw = await _httpClient.GetStringAsync(url);

            return VideoWatchPageParser.Initialize(raw);
        }

        private async Task<VideoInfoParser> GetVideoInfoParserAsync(string videoId, string el, string sts)
        {
            // This parameter does magic and a lot of videos don't work without it
            var eurl = $"https://youtube.googleapis.com/v/{videoId}".UrlEncode();

            var url = $"https://www.youtube.com/get_video_info?video_id={videoId}&el={el}&sts={sts}&eurl={eurl}&hl=en";
            var raw = await _httpClient.GetStringAsync(url);

            return VideoInfoParser.Initialize(raw);
        }

        private async Task<VideoInfoParser> GetVideoInfoParserAsync(string videoId, string sts = null)
        {
            // Get video info parser with "el=embedded"
            var videoInfoParser = await GetVideoInfoParserAsync(videoId, "embedded", sts);

            // If video info doesn't contain video ID, which means it's unavailable - throw
            if (videoInfoParser.ParseId().IsNullOrWhiteSpace())
                throw new VideoUnavailableException(videoId, $"Video [{videoId}] is unavailable.");

            // If the video is playable - return
            if (videoInfoParser.ParseErrorReason().IsNullOrWhiteSpace())
                return videoInfoParser;

            // If we don't need to ensure that the video is playable - return
            if (sts.IsNullOrWhiteSpace())
                return videoInfoParser;

            // If video requires purchase - throw
            var previewVideoId = videoInfoParser.ParsePreviewVideoId();
            if (!previewVideoId.IsNullOrWhiteSpace())
            {
                throw new VideoRequiresPurchaseException(videoId, previewVideoId,
                    $"Video [{videoId}] is unplayable because it requires purchase.");
            }

            // Get video info parser with "el=detailpage"
            videoInfoParser = await GetVideoInfoParserAsync(videoId, "detailpage", sts);

            // If the video is still unplayable - throw
            var errorReason = videoInfoParser.ParseErrorReason();
            if (!errorReason.IsNullOrWhiteSpace())
                throw new VideoUnplayableException(videoId, $"Video [{videoId}] is unplayable. (Reason: {errorReason})");

            return videoInfoParser;
        }

        private async Task<PlayerSourceParser> GetPlayerSourceParserAsync(string sourceUrl)
        {
            var raw = await _httpClient.GetStringAsync(sourceUrl);
            return PlayerSourceParser.Initialize(raw);
        }

        private async Task<DashManifestParser> GetDashManifestParserAsync(string dashManifestUrl)
        {
            var raw = await _httpClient.GetStringAsync(dashManifestUrl);
            return DashManifestParser.Initialize(raw);
        }

        private async Task<ClosedCaptionTrackAjaxParser> GetClosedCaptionTrackAjaxParserAsync(string url)
        {
            var raw = await _httpClient.GetStringAsync(url);
            return ClosedCaptionTrackAjaxParser.Initialize(raw);
        }

        private async Task<ChannelPageParser> GetChannelPageParserAsync(string channelId)
        {
            var url = $"https://www.youtube.com/channel/{channelId}?hl=en";

            // Retry up to 5 times because sometimes the response has random errors
            for (var retry = 0; retry < 5; retry++)
            {
                var raw = await _httpClient.GetStringAsync(url);
                var parser = ChannelPageParser.Initialize(raw);

                // If successful - return
                if (parser.ParseIsAvailable())
                    return parser;

                // Otherwise put a delay before trying again
                await Task.Delay(150);
            }

            // Throw exception
            throw new InvalidDataException("Could not get channel page.");
        }

        private async Task<ChannelPageParser> GetChannelPageParserByUsernameAsync(string username)
        {
            username = username.UrlEncode();

            var url = $"https://www.youtube.com/user/{username}?hl=en";

            // Retry up to 5 times because sometimes the response has random errors
            for (var retry = 0; retry < 5; retry++)
            {
                var raw = await _httpClient.GetStringAsync(url);
                var parser = ChannelPageParser.Initialize(raw);

                // If successful - return
                if (parser.ParseIsAvailable())
                    return parser;

                // Otherwise put a delay before trying again
                await Task.Delay(150);
            }

            // Throw exception
            throw new InvalidDataException("Could not get channel page.");
        }

        private async Task<PlaylistAjaxParser> GetPlaylistAjaxParserAsync(string playlistId, int index)
        {
            var url = $"https://www.youtube.com/list_ajax?style=json&action_get_list=1&list={playlistId}&index={index}&hl=en";
            var raw = await _httpClient.GetStringAsync(url);

            return PlaylistAjaxParser.Initialize(raw);
        }

        private async Task<PlaylistAjaxParser> GetPlaylistAjaxParserForSearchAsync(string query, int page)
        {
            query = query.UrlEncode();

            // Don't ensure success here so that empty pages could be parsed

            var url = $"https://www.youtube.com/search_ajax?style=json&search_query={query}&page={page}&hl=en";
            var raw = await _httpClient.GetStringAsync(url, false);

            return PlaylistAjaxParser.Initialize(raw);
        }
    }
}