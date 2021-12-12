using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using YoutubeExplode.Utils;
using YoutubeExplode.Utils.Extensions;

namespace YoutubeExplode.Bridge;

internal class SearchResultPlaylistExtractor
{
    private readonly JsonElement _content;
    private readonly Memo _memo = new();

    public SearchResultPlaylistExtractor(JsonElement content) =>
        _content = content;

    public string? TryGetPlaylistId() => _memo.Wrap(() =>
        _content
            .GetPropertyOrNull("playlistId")?
            .GetStringOrNull()
    );

    public string? TryGetPlaylistTitle() => _memo.Wrap(() =>
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

    private JsonElement? TryGetPlaylistAuthorDetails() => _memo.Wrap(() =>
        _content
            .GetPropertyOrNull("longBylineText")?
            .GetPropertyOrNull("runs")?
            .EnumerateArrayOrNull()?
            .ElementAtOrNull(0)
    );

    public string? TryGetPlaylistAuthor() => _memo.Wrap(() =>
        TryGetPlaylistAuthorDetails()?
            .GetPropertyOrNull("text")?
            .GetStringOrNull()
    );

    public string? TryGetPlaylistChannelId() => _memo.Wrap(() =>
        TryGetPlaylistAuthorDetails()?
            .GetPropertyOrNull("navigationEndpoint")?
            .GetPropertyOrNull("browseEndpoint")?
            .GetPropertyOrNull("browseId")?
            .GetStringOrNull()
    );

    public IReadOnlyList<ThumbnailExtractor> GetPlaylistThumbnails() => _memo.Wrap(() =>
        _content
            .GetPropertyOrNull("thumbnails")?
            .EnumerateDescendantProperties("thumbnails")
            .SelectMany(j => j.EnumerateArrayOrEmpty())
            .Select(j => new ThumbnailExtractor(j))
            .ToArray() ??

        Array.Empty<ThumbnailExtractor>()
    );
}