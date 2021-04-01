using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp.Html.Dom;
using YoutubeExplode.Channels;
using YoutubeExplode.Exceptions;
using YoutubeExplode.Utils;
using YoutubeExplode.Videos;

namespace YoutubeExplode.Common
{
    internal abstract class ResolverBase
    {
        private readonly HttpClient _httpClient;

        protected ResolverBase(HttpClient httpClient) => _httpClient = httpClient;

        private async ValueTask<string> GetStringAsync(
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
            request.Headers.Add(
                "Cookie",
                "CONSENT=YES+cb"
            );

            using var response = await _httpClient.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken
            );

            // Check for rate limit
            if ((int) response.StatusCode == 429)
            {
                throw RequestLimitExceededException.FailedHttpRequest(response);
            }

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

        private async ValueTask<IHtmlDocument> GetChannelPageAsync(
            string channelRoute,
            CancellationToken cancellationToken = default)
        {
            var url = $"https://www.youtube.com/{channelRoute}?hl=en";
            var raw = await GetStringAsync(url, cancellationToken);

            return Html.Parse(raw);
        }

        protected async ValueTask<IHtmlDocument> GetChannelPageAsync(
            ChannelId channelId,
            CancellationToken cancellationToken = default) =>
            await GetChannelPageAsync("channel/" + channelId, cancellationToken);

        protected async ValueTask<IHtmlDocument> GetChannelPageAsync(
            UserName userName,
            CancellationToken cancellationToken = default) =>
            await GetChannelPageAsync("user/" + userName, cancellationToken);

        protected async ValueTask<IHtmlDocument> GetWatchPageAsync(
            VideoId videoId,
            CancellationToken cancellationToken = default)
        {
            var url = $"https://youtube.com/watch?v={videoId}&bpctr=9999999999&hl=en";
            var raw = await GetStringAsync(url, cancellationToken);

            return Html.Parse(raw);
        }

        protected async ValueTask<IReadOnlyDictionary<string, string>> GetVideoInfoAsync(
            VideoId videoId,
            string signatureTimestamp,
            CancellationToken cancellationToken = default)
        {
            var eurl = WebUtility.HtmlEncode($"https://youtube.googleapis.com/v/{videoId}");

            var url =
                "https://youtube.com/get_video_info" +
                $"?video_id={videoId}" +
                $"&el=embedded" +
                $"&sts={signatureTimestamp}" +
                $"&eurl={eurl}" +
                "&hl=en";

            var raw = await GetStringAsync(url, cancellationToken);

            return Url.SplitQuery(raw);
        }
    }
}