using System;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using YoutubeExplode.Utils;
using YoutubeExplode.Utils.Extensions;

namespace YoutubeExplode.Bridge;

internal partial class VideoWatchPage
{
    private readonly IHtmlDocument _content;

    public bool IsAvailable => Memo.Cache(this, () =>
        _content.QuerySelector("meta[property=\"og:url\"]") is not null
    );

    public DateTimeOffset? UploadDate => Memo.Cache(this, () =>
        _content
            .QuerySelector("meta[itemprop=\"datePublished\"]")?
            .GetAttribute("content")?
            .NullIfWhiteSpace()?
            .ParseDateTimeOffsetOrNull(new[] { @"yyyy-MM-dd" })
    );

    public long? LikeCount => Memo.Cache(this, () =>
        _content
            .Source
            .Text
            .Pipe(s => Regex.Match(
                s,
                """
                "label"\s*:\s*"([\d,\.]+) likes"
                """
            ).Groups[1].Value)
            .NullIfWhiteSpace()?
            .StripNonDigit()
            .ParseLongOrNull()
    );

    public long? DislikeCount => Memo.Cache(this, () =>
        _content
            .Source
            .Text
            .Pipe(s => Regex.Match(
                s,
                """
                "label"\s*:\s*"([\d,\.]+) dislikes"
                """
            ).Groups[1].Value)
            .NullIfWhiteSpace()?
            .StripNonDigit()
            .ParseLongOrNull()
    );

    private JsonElement? PlayerConfig => Memo.Cache(this, () =>
        _content
            .GetElementsByTagName("script")
            .Select(e => e.Text())
            .Select(s => Regex.Match(s, @"ytplayer\.config\s*=\s*(\{.*\})").Groups[1].Value)
            .FirstOrDefault(s => !string.IsNullOrWhiteSpace(s))?
            .NullIfWhiteSpace()?
            .Pipe(Json.Extract)
            .Pipe(Json.TryParse)
    );

    public PlayerResponse? PlayerResponse => Memo.Cache(this, () =>
        _content
            .GetElementsByTagName("script")
            .Select(e => e.Text())
            .Select(s => Regex.Match(s, @"var\s+ytInitialPlayerResponse\s*=\s*(\{.*\})").Groups[1].Value)
            .FirstOrDefault(s => !string.IsNullOrWhiteSpace(s))?
            .NullIfWhiteSpace()?
            .Pipe(Json.Extract)
            .Pipe(Json.TryParse)?
            .Pipe(j => new PlayerResponse(j)) ??

        PlayerConfig?
            .GetPropertyOrNull("args")?
            .GetPropertyOrNull("player_response")?
            .GetStringOrNull()?
            .Pipe(Json.TryParse)?
            .Pipe(j => new PlayerResponse(j))
    );

    public VideoWatchPage(IHtmlDocument content) => _content = content;
}

internal partial class VideoWatchPage
{
    public static VideoWatchPage? TryParse(string raw)
    {
        var content = Html.Parse(raw);

        if (content.Body?.QuerySelector("#player") is null)
            return null;

        return new VideoWatchPage(content);
    }
}