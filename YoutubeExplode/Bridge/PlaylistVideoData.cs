using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Lazy;
using YoutubeExplode.Utils.Extensions;

namespace YoutubeExplode.Bridge;

internal class PlaylistVideoData
{
    private readonly JsonElement _content;

    [Lazy]
    public int? Index => _content
        .GetPropertyOrNull("navigationEndpoint")?
        .GetPropertyOrNull("watchEndpoint")?
        .GetPropertyOrNull("index")?
        .GetInt32OrNull();

    [Lazy]
    public string? Id => _content.GetPropertyOrNull("videoId")?.GetStringOrNull();

    [Lazy]
    public string? Title =>
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
            .ConcatToString();

    [Lazy]
    private JsonElement? AuthorDetails =>
        _content
            .GetPropertyOrNull("longBylineText")?
            .GetPropertyOrNull("runs")?
            .EnumerateArrayOrNull()?
            .ElementAtOrNull(0) ??

        _content
            .GetPropertyOrNull("shortBylineText")?
            .GetPropertyOrNull("runs")?
            .EnumerateArrayOrNull()?
            .ElementAtOrNull(0);

    [Lazy]
    public string? Author => AuthorDetails?.GetPropertyOrNull("text")?.GetStringOrNull();

    [Lazy]
    public string? ChannelId => AuthorDetails?
        .GetPropertyOrNull("navigationEndpoint")?
        .GetPropertyOrNull("browseEndpoint")?
        .GetPropertyOrNull("browseId")?
        .GetStringOrNull();

    [Lazy]
    public TimeSpan? Duration =>
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
            .ParseTimeSpanOrNull(new[] { @"m\:ss", @"mm\:ss", @"h\:mm\:ss", @"hh\:mm\:ss" });

    [Lazy]
    public IReadOnlyList<ThumbnailData> Thumbnails => _content
        .GetPropertyOrNull("thumbnail")?
        .GetPropertyOrNull("thumbnails")?
        .EnumerateArrayOrNull()?
        .Select(j => new ThumbnailData(j))
        .ToArray() ?? Array.Empty<ThumbnailData>();

    public PlaylistVideoData(JsonElement content) => _content = content;
}