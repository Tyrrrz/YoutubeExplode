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

    public SearchResultPlaylistExtractor(JsonElement content) =>
        _content = content;

    public string? TryGetPlaylistId() => Memo.Cache(this, () =>
        _content
            .GetPropertyOrNull("playlistId")?
            .GetStringOrNull()
    );

    public string? TryGetPlaylistTitle() => Memo.Cache(this, () =>
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

    private JsonElement? TryGetPlaylistAuthorDetails() => Memo.Cache(this, () =>
        _content
            .GetPropertyOrNull("longBylineText")?
            .GetPropertyOrNull("runs")?
            .EnumerateArrayOrNull()?
            .ElementAtOrNull(0)
    );

    public string? TryGetPlaylistAuthor() => Memo.Cache(this, () =>
        TryGetPlaylistAuthorDetails()?
            .GetPropertyOrNull("text")?
            .GetStringOrNull()
    );

    public string? TryGetPlaylistChannelId() => Memo.Cache(this, () =>
        TryGetPlaylistAuthorDetails()?
            .GetPropertyOrNull("navigationEndpoint")?
            .GetPropertyOrNull("browseEndpoint")?
            .GetPropertyOrNull("browseId")?
            .GetStringOrNull()
    );

    public IReadOnlyList<ThumbnailExtractor> GetPlaylistThumbnails() => Memo.Cache(this, () =>
        _content
            .GetPropertyOrNull("thumbnails")?
            .EnumerateDescendantProperties("thumbnails")
            .SelectMany(j => j.EnumerateArrayOrEmpty())
            .Select(j => new ThumbnailExtractor(j))
            .ToArray() ??

        Array.Empty<ThumbnailExtractor>()
    );
}