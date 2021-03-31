using System;
using System.Net.Http;
using System.Threading.Tasks;
using AngleSharp.Html.Dom;
using YoutubeExplode.Utils;
using YoutubeExplode.Utils.Extensions;

namespace YoutubeExplode.Channels.Resolving
{
    internal class ChannelResolver
    {
        private readonly HttpClient _httpClient;
        private readonly string _channelRoute;
        private readonly Cache _cache = new();

        private ChannelResolver(HttpClient httpClient, string channelRoute)
        {
            _httpClient = httpClient;
            _channelRoute = channelRoute;
        }

        public ChannelResolver(HttpClient httpClient, ChannelId channelId)
            : this(httpClient, "channel/" + channelId)
        {
        }

        public ChannelResolver(HttpClient httpClient, UserName userName)
            : this(httpClient, "user/" + userName)
        {
        }

        private ValueTask<IHtmlDocument> GetChannelPageAsync() => _cache.WrapAsync(async () =>
        {
            var url = $"https://www.youtube.com/{_channelRoute}?hl=en";
            var raw = await _httpClient.GetStringAsync(url);

            return Html.Parse(raw);
        });

        public ValueTask<string> GetChannelUrlAsync() => _cache.WrapAsync(async () =>
        {
            var channelPage = await GetChannelPageAsync();

            return channelPage
                .QuerySelectorOrThrow("meta[property=\"og:url\"]")
                .GetAttributeOrThrow("content");
        });

        public ValueTask<ChannelId> GetChannelIdAsync() => _cache.WrapAsync(async () =>
        {
            var channelUrl = await GetChannelUrlAsync();

            return (ChannelId) channelUrl.SubstringAfter("channel/", StringComparison.OrdinalIgnoreCase);
        });

        public ValueTask<string> GetChannelTitleAsync() => _cache.WrapAsync(async () =>
        {
            var channelPage = await GetChannelPageAsync();

            return channelPage
                .QuerySelectorOrThrow("meta[property=\"og:title\"]")
                .GetAttributeOrThrow("content");
        });

        public ValueTask<string> GetChannelLogoUrlAsync() => _cache.WrapAsync(async () =>
        {
            var channelPage = await GetChannelPageAsync();

            return channelPage
                .QuerySelectorOrThrow("meta[property=\"og:image\"]")
                .GetAttributeOrThrow("content");
        });
    }
}