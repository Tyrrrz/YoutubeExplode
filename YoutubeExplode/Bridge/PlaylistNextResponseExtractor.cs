using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using YoutubeExplode.Utils;
using YoutubeExplode.Utils.Extensions;

namespace YoutubeExplode.Bridge;

internal partial class PlaylistNextResponseExtractor : IPlaylistExtractor
{
    private readonly JsonElement _content;

    public PlaylistNextResponseExtractor(JsonElement content) => _content = content;

    public bool IsPlaylistAvailable() => Memo.Cache(this, () =>
        TryGetPlaylistRoot() is not null
    );

    private JsonElement? TryGetPlaylistRoot() => Memo.Cache(this, () =>
        _content
            .GetPropertyOrNull("contents")?
            .GetPropertyOrNull("twoColumnWatchNextResults")?
            .GetPropertyOrNull("playlist")?
            .GetPropertyOrNull("playlist")
    );

    public string? TryGetPlaylistTitle() => Memo.Cache(this, () =>
        TryGetPlaylistRoot()?
            .GetPropertyOrNull("title")?
            .GetStringOrNull()
    );

    public string? TryGetPlaylistAuthor() => Memo.Cache(this, () =>
        TryGetPlaylistRoot()?
            .GetPropertyOrNull("ownerName")?
            .GetPropertyOrNull("simpleText")?
            .GetStringOrNull()
    );

    public string? TryGetPlaylistChannelId() => null;

    public string? TryGetPlaylistDescription() => null;

    public IReadOnlyList<ThumbnailExtractor> GetPlaylistThumbnails() => Memo.Cache(this, () =>
        GetVideos()
            .FirstOrDefault()?
            .GetVideoThumbnails() ??

        Array.Empty<ThumbnailExtractor>()
    );

    public IReadOnlyList<PlaylistVideoExtractor> GetVideos() => Memo.Cache(this, () =>
        TryGetPlaylistRoot()?
            .GetPropertyOrNull("contents")?
            .EnumerateArrayOrNull()?
            .Select(j => j.GetPropertyOrNull("playlistPanelVideoRenderer"))
            .WhereNotNull()
            .Select(j => new PlaylistVideoExtractor(j))
            .ToArray() ??

        Array.Empty<PlaylistVideoExtractor>()
    );

    public string? TryGetVisitorData() => Memo.Cache(this, () =>
        _content
            .GetPropertyOrNull("responseContext")?
            .GetPropertyOrNull("visitorData")?
            .GetStringOrNull()
    );
}

internal partial class PlaylistNextResponseExtractor
{
    public static PlaylistNextResponseExtractor Create(string raw) => new(Json.Parse(raw));
}