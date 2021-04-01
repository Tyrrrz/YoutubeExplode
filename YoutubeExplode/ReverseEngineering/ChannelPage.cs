using System;
using System.Threading.Tasks;
using AngleSharp.Html.Dom;
using YoutubeExplode.Exceptions;
using YoutubeExplode.Utils;
using YoutubeExplode.Utils.Extensions;

namespace YoutubeExplode.ReverseEngineering
{
    internal class ChannelPage
    {
        private readonly string _raw;
        private readonly Cache _cache = new();

        public ChannelPage(string raw) => _raw = raw;

        private IHtmlDocument GetHtml() => _cache.Wrap(() => Html.Parse(_raw));

        public string GetChannelUrl() => _cache.Wrap(() =>
            GetHtml()
                .QuerySelectorOrThrow("meta[property=\"og:url\"]")
                .GetAttributeOrThrow("content")
        );

        public string GetChannelId() => _cache.Wrap(() =>
            GetChannelUrl().SubstringAfter("channel/", StringComparison.OrdinalIgnoreCase)
        );

        public string GetChannelTitle() => _cache.Wrap(() =>
            GetHtml()
                .QuerySelectorOrThrow("meta[property=\"og:title\"]")
                .GetAttributeOrThrow("content")
        );

        public string GetChannelLogoUrl() => _cache.Wrap(() =>
            GetHtml()
                .QuerySelectorOrThrow("meta[property=\"og:image\"]")
                .GetAttributeOrThrow("content")
        );
    }
}