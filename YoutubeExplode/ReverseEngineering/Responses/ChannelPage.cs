using System;
using System.Net.Http;
using System.Threading.Tasks;
using AngleSharp.Html.Dom;
using YoutubeExplode.Internal;
using YoutubeExplode.Internal.Extensions;

namespace YoutubeExplode.ReverseEngineering.Responses
{
    internal partial class ChannelPage
    {
        private readonly IHtmlDocument _root;

        public ChannelPage(IHtmlDocument root)
        {
            _root = root;
        }

        public string GetChannelUrl() => _root
            .QuerySelector("meta[property=\"og:url\"]")
            .GetAttribute("content");

        public string GetChannelId() => GetChannelUrl()
            .SubstringAfter("channel/", StringComparison.OrdinalIgnoreCase);

        public string GetChannelTitle() => _root
            .QuerySelector("meta[property=\"og:title\"]")
            .GetAttribute("content");

        public string GetChannelLogoUrl() => _root
            .QuerySelector("meta[property=\"og:image\"]")
            .GetAttribute("content");
    }

    internal partial class ChannelPage
    {
        public static ChannelPage Parse(string raw) => new ChannelPage(Html.Parse(raw));

        public static async Task<ChannelPage> GetAsync(HttpClient httpClient, string id) =>
            await Retry.WrapAsync(async () =>
            {
                var url = $"https://www.youtube.com/channel/{id}?hl=en";
                var raw = await httpClient.GetStringAsync(url);

                return Parse(raw);
            });

        public static async Task<ChannelPage> GetByUserNameAsync(HttpClient httpClient, string userName) =>
            await Retry.WrapAsync(async () =>
            {
                var url = $"https://www.youtube.com/user/{userName}?hl=en";
                var raw = await httpClient.GetStringAsync(url);

                return Parse(raw);
            });
    }
}