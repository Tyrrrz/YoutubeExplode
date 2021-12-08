using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using YoutubeExplode.Utils;
using YoutubeExplode.Utils.Extensions;

namespace YoutubeExplode.Bridge.Extractors;

internal class PlaylistVideoExtractor
{
    private static readonly string[] DurationFormats = {@"m\:ss", @"mm\:ss", @"h\:mm\:ss", @"hh\:mm\:ss"};

    private readonly JsonElement _content;
    private readonly Memo _memo = new();

    public PlaylistVideoExtractor(JsonElement content) => _content = content;

    public string? TryGetVideoId() => _memo.Wrap(() =>
        _content
            .GetPropertyOrNull("videoId")?
            .GetStringOrNull()
    );

    public string? TryGetVideoTitle() => _memo.Wrap(() =>
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

    private JsonElement? TryGetAuthorDetails() => _memo.Wrap(() =>
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

    public string? TryGetVideoAuthor() => _memo.Wrap(() =>
        TryGetAuthorDetails()?
            .GetPropertyOrNull("text")?
            .GetStringOrNull()
    );

    public string? TryGetVideoChannelId() => _memo.Wrap(() =>
        TryGetAuthorDetails()?
            .GetPropertyOrNull("navigationEndpoint")?
            .GetPropertyOrNull("browseEndpoint")?
            .GetPropertyOrNull("browseId")?
            .GetStringOrNull()
    );

    public TimeSpan? TryGetVideoDuration() => _memo.Wrap(() =>
        _content
            .GetPropertyOrNull("lengthSeconds")?
            .GetStringOrNull()?
            .ParseDoubleOrNull()?
            .Pipe(TimeSpan.FromSeconds) ??

        _content
            .GetPropertyOrNull("lengthText")?
            .GetPropertyOrNull("simpleText")?
            .GetStringOrNull()?
            .ParseTimeSpanOrNull(DurationFormats) ??

        _content
            .GetPropertyOrNull("lengthText")?
            .GetPropertyOrNull("runs")?
            .EnumerateArrayOrNull()?
            .Select(j => j.GetPropertyOrNull("text")?.GetStringOrNull())
            .WhereNotNull()
            .ConcatToString()
            .ParseTimeSpanOrNull(DurationFormats)
    );

    public IReadOnlyList<ThumbnailExtractor> GetVideoThumbnails() => _memo.Wrap(() =>
        _content
            .GetPropertyOrNull("thumbnail")?
            .GetPropertyOrNull("thumbnails")?
            .EnumerateArrayOrNull()?
            .Select(j => new ThumbnailExtractor(j))
            .ToArray() ??

        Array.Empty<ThumbnailExtractor>()
    );
}