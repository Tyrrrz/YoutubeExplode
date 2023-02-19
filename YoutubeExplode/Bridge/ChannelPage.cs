using System;
using AngleSharp.Html.Dom;
using YoutubeExplode.Utils;
using YoutubeExplode.Utils.Extensions;

namespace YoutubeExplode.Bridge;

internal partial class ChannelPage
{
    private readonly IHtmlDocument _content;

    public string? Url => Memo.Cache(this, () =>
        _content
            .QuerySelector("meta[property=\"og:url\"]")?
            .GetAttribute("content")
    );

    public string? Id => Memo.Cache(this, () =>
        Url?.SubstringAfter("channel/", StringComparison.OrdinalIgnoreCase)
    );

    public string? Title => Memo.Cache(this, () =>
        _content
            .QuerySelector("meta[property=\"og:title\"]")?
            .GetAttribute("content")
    );

    public string? LogoUrl => Memo.Cache(this, () =>
        _content
            .QuerySelector("meta[property=\"og:image\"]")?
            .GetAttribute("content")
    );

    public ChannelPage(IHtmlDocument content) => _content = content;
}

internal partial class ChannelPage
{
    public static ChannelPage? TryParse(string raw)
    {
        var content = Html.Parse(raw);

        if (content.QuerySelector("meta[property=\"og:url\"]") is null)
            return null;

        return new ChannelPage(content);
    }
}