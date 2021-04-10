using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using YoutubeExplode.Bridge.Extractors;
using YoutubeExplode.Channels;
using YoutubeExplode.Exceptions;
using YoutubeExplode.Utils;
using YoutubeExplode.Videos;

namespace YoutubeExplode.Bridge
{
    internal class YoutubeController
    {
        private readonly HttpClient _httpClient;

        public YoutubeController(HttpClient httpClient) => _httpClient = httpClient;

        private async ValueTask<string> SendHttpRequestAsync(
            string url,
            CancellationToken cancellationToken = default)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, url);

            // User-agent
            if (!request.Headers.Contains("User-Agent"))
            {
                request.Headers.Add(
                    "User-Agent",
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/86.0.4240.111 Safari/537.36"
                );
            }

            // Consent cookie
            request.Headers.Add("Cookie", "CONSENT=YES+cb");

            using var response = await _httpClient.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken
            );

            // Special case check for rate limiting errors
            if ((int) response.StatusCode == 429)
            {
                throw new RequestLimitExceededException(
                    "Exceeded request limit. " +
                    "Please try again in a few hours. " +
                    "Alternatively, inject an instance of HttpClient that includes cookies for an authenticated user."
                );
            }

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync(cancellationToken);
        }

        private async ValueTask<ChannelPageExtractor> GetChannelPageAsync(
            string channelRoute,
            CancellationToken cancellationToken = default)
        {
            var url = $"https://www.youtube.com/{channelRoute}?hl=en";

            for (var retry = 0; retry <= 5; retry++)
            {
                var raw = await SendHttpRequestAsync(url, cancellationToken);

                var channelPage = ChannelPageExtractor.TryCreate(raw);
                if (channelPage is not null)
                    return channelPage;
            }

            throw new YoutubeExplodeException(
                "Channel page is broken. " +
                "Please try again in a few minutes."
            );
        }

        public async ValueTask<ChannelPageExtractor> GetChannelPageAsync(
            ChannelId channelId,
            CancellationToken cancellationToken = default) =>
            await GetChannelPageAsync("channel/" + channelId, cancellationToken);

        public async ValueTask<ChannelPageExtractor> GetChannelPageAsync(
            UserName userName,
            CancellationToken cancellationToken = default) =>
            await GetChannelPageAsync("user/" + userName, cancellationToken);

        public async ValueTask<VideoWatchPageExtractor> GetVideoWatchPageAsync(
            VideoId videoId,
            CancellationToken cancellationToken = default)
        {
            var url = $"https://youtube.com/watch?v={videoId}&bpctr=9999999999&hl=en";

            for (var retry = 0; retry <= 5; retry++)
            {
                var raw = await SendHttpRequestAsync(url, cancellationToken);

                var watchPage = VideoWatchPageExtractor.TryCreate(raw);
                if (watchPage is not null)
                {
                    if (!watchPage.IsVideoAvailable())
                    {
                        throw new VideoUnavailableException($"Video '{videoId}' is not available.");
                    }

                    return watchPage;
                }
            }

            throw new YoutubeExplodeException(
                "Video watch page is broken. " +
                "Please try again in a few minutes."
            );
        }

        public async ValueTask<VideoInfoExtractor> GetVideoInfoAsync(
            VideoId videoId,
            string signatureTimestamp,
            CancellationToken cancellationToken = default)
        {
            var eurl = WebUtility.HtmlEncode($"https://youtube.googleapis.com/v/{videoId}");

            var url =
                "https://youtube.com/get_video_info" +
                $"?video_id={videoId}" +
                "&el=embedded" +
                $"&sts={signatureTimestamp}" +
                $"&eurl={eurl}" +
                "&hl=en";

            var raw = await SendHttpRequestAsync(url, cancellationToken);

            var videoInfo = VideoInfoExtractor.Create(raw);

            if (!videoInfo.IsVideoAvailable())
            {
                throw new VideoUnavailableException($"Video '{videoId}' is not available.");
            }

            return videoInfo;
        }

        public async ValueTask<VideoInfoExtractor> GetVideoInfoAsync(
            VideoId videoId,
            CancellationToken cancellationToken = default) =>
            await GetVideoInfoAsync(videoId, "", cancellationToken);

        public async ValueTask<ClosedCaptionTrackExtractor> GetClosedCaptionTrackAsync(
            string url,
            CancellationToken cancellationToken = default)
        {
            // Enforce known format
            var urlWithFormat = Url.SetQueryParameter(url, "format", "3");

            var raw = await SendHttpRequestAsync(urlWithFormat, cancellationToken);

            return ClosedCaptionTrackExtractor.Create(raw);
        }

        public async ValueTask<PlayerSourceExtractor> GetPlayerSourceAsync(
            string url,
            CancellationToken cancellationToken = default)
        {
            var raw = await SendHttpRequestAsync(url, cancellationToken);
            return PlayerSourceExtractor.Create(raw);
        }

        public async ValueTask<DashManifestExtractor> GetDashManifestAsync(
            string url,
            CancellationToken cancellationToken = default)
        {
            var raw = await SendHttpRequestAsync(url, cancellationToken);
            return DashManifestExtractor.Create(raw);
        }
    }
}