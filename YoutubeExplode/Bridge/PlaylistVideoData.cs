using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using YoutubeExplode.Utils;
using YoutubeExplode.Utils.Extensions;

namespace YoutubeExplode.Bridge;

internal class PlaylistVideoData
{
    private readonly JsonElement _content;

    public int? Index => Memo.Cache(this, () =>
        _content
            .GetPropertyOrNull("navigationEndpoint")?
            .GetPropertyOrNull("watchEndpoint")?
            .GetPropertyOrNull("index")?
            .GetInt32OrNull()
    );

    public string? Id => Memo.Cache(this, () =>
        _content
            .GetPropertyOrNull("videoId")?
            .GetStringOrNull()
    );

    public string? Title => Memo.Cache(this, () =>
        _content
            .GetPropertyOrNull("title")?
            .GetPropertyOrNull("simpleText")?
            .GetStringOrNull() ??

        _content
            .GetPropertyOrNull("title")?
            .GetPropertyOrNull("runs")?
            .EnumerateArrayOrNull()?
            .Select(j => j.GetPropertyOrNull("text")?.GetStringOrNull())
            .WhereNotNull()
            .ConcatToString()
    );

    private JsonElement? AuthorDetails => Memo.Cache(this, () =>
        _content
            .GetPropertyOrNull("longBylineText")?
            .GetPropertyOrNull("runs")?
            .EnumerateArrayOrNull()?
            .ElementAtOrNull(0) ??

        _content
            .GetPropertyOrNull("shortBylineText")?
            .GetPropertyOrNull("runs")?
            .EnumerateArrayOrNull()?
            .ElementAtOrNull(0)
    );

    public string? Author => Memo.Cache(this, () =>
        AuthorDetails?
            .GetPropertyOrNull("text")?
            .GetStringOrNull()
    );

    public string? ChannelId => Memo.Cache(this, () =>
        AuthorDetails?
            .GetPropertyOrNull("navigationEndpoint")?
            .GetPropertyOrNull("browseEndpoint")?
            .GetPropertyOrNull("browseId")?
            .GetStringOrNull()
    );

    public TimeSpan? Duration => Memo.Cache(this, () =>
        _content
            .GetPropertyOrNull("lengthSeconds")?
            .GetStringOrNull()?
            .ParseDoubleOrNull()?
            .Pipe(TimeSpan.FromSeconds) ??

        _content
            .GetPropertyOrNull("lengthText")?
            .GetPropertyOrNull("simpleText")?
            .GetStringOrNull()?
            .ParseTimeSpanOrNull(new[] { @"m\:ss", @"mm\:ss", @"h\:mm\:ss", @"hh\:mm\:ss" }) ??

        _content
            .GetPropertyOrNull("lengthText")?
            .GetPropertyOrNull("runs")?
            .EnumerateArrayOrNull()?
            .Select(j => j.GetPropertyOrNull("text")?.GetStringOrNull())
            .WhereNotNull()
            .ConcatToString()
            .ParseTimeSpanOrNull(new[] { @"m\:ss", @"mm\:ss", @"h\:mm\:ss", @"hh\:mm\:ss" })
    );

    public IReadOnlyList<ThumbnailData> Thumbnails => Memo.Cache(this, () =>
        _content
            .GetPropertyOrNull("thumbnail")?
            .GetPropertyOrNull("thumbnails")?
            .EnumerateArrayOrNull()?
            .Select(j => new ThumbnailData(j))
            .ToArray() ??

        Array.Empty<ThumbnailData>()
    );

    public PlaylistVideoData(JsonElement content) => _content = content;
}