using System;
using AngleSharp.Html.Dom;
using YoutubeExplode.Utils;
using YoutubeExplode.Utils.Extensions;

namespace YoutubeExplode.Extraction.Responses
{
    internal class ChannelPage
    {
        private readonly IHtmlDocument _root;
        private readonly Memo _memo = new();

        public ChannelPage(IHtmlDocument root) => _root = root;

        private bool IsValid() => _memo.Wrap(() =>
            _root.QuerySelector("meta[property=\"og:url\"]") is not null
        );

        public string? TryGetChannelUrl() => _memo.Wrap(() =>
            _root
                .QuerySelector("meta[property=\"og:url\"]")?
                .GetAttribute("content")
        );

        public string? TryGetChannelId() => _memo.Wrap(() =>
            TryGetChannelUrl()?.SubstringAfter("channel/", StringComparison.OrdinalIgnoreCase)
        );

        public string? TryGetChannelTitle() => _memo.Wrap(() =>
            _root
                .QuerySelector("meta[property=\"og:title\"]")?
                .GetAttribute("content")
        );

        public string? TryGetChannelLogoUrl() => _memo.Wrap(() =>
            _root
                .QuerySelector("meta[property=\"og:image\"]")?
                .GetAttribute("content")
        );
    }
}