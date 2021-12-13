using System;
using AngleSharp.Html.Dom;
using YoutubeExplode.Utils;
using YoutubeExplode.Utils.Extensions;

namespace YoutubeExplode.Bridge;

internal partial class ChannelPageExtractor
{
    private readonly IHtmlDocument _content;

    public ChannelPageExtractor(IHtmlDocument content) => _content = content;

    public string? TryGetChannelUrl() => Memo.Cache(this, () =>
        _content
            .QuerySelector("meta[property=\"og:url\"]")?
            .GetAttribute("content")
    );

    public string? TryGetChannelId() => Memo.Cache(this, () =>
        TryGetChannelUrl()?.SubstringAfter("channel/", StringComparison.OrdinalIgnoreCase)
    );

    public string? TryGetChannelTitle() => Memo.Cache(this, () =>
        _content
            .QuerySelector("meta[property=\"og:title\"]")?
            .GetAttribute("content")
    );

    public string? TryGetChannelLogoUrl() => Memo.Cache(this, () =>
        _content
            .QuerySelector("meta[property=\"og:image\"]")?
            .GetAttribute("content")
    );
}

internal partial class ChannelPageExtractor
{
    public static ChannelPageExtractor? TryCreate(string raw)
    {
        var content = Html.Parse(raw);

        var isValid = content.QuerySelector("meta[property=\"og:url\"]") is not null;
        if (!isValid)
            return null;

        return new ChannelPageExtractor(content);
    }
}