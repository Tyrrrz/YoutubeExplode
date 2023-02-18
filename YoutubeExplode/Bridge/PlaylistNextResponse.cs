using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using YoutubeExplode.Utils;
using YoutubeExplode.Utils.Extensions;

namespace YoutubeExplode.Bridge;

internal partial class PlaylistNextResponse : IPlaylistData
{
    private readonly JsonElement _content;

    private JsonElement? ContentRoot => Memo.Cache(this, () =>
        _content
            .GetPropertyOrNull("contents")?
            .GetPropertyOrNull("twoColumnWatchNextResults")?
            .GetPropertyOrNull("playlist")?
            .GetPropertyOrNull("playlist")
    );

    public bool IsAvailable => Memo.Cache(this, () =>
        ContentRoot is not null
    );

    public string? Title => Memo.Cache(this, () =>
        ContentRoot?
            .GetPropertyOrNull("title")?
            .GetStringOrNull()
    );

    public string? Author => Memo.Cache(this, () =>
        ContentRoot?
            .GetPropertyOrNull("ownerName")?
            .GetPropertyOrNull("simpleText")?
            .GetStringOrNull()
    );

    public string? ChannelId => null;

    public string? Description => null;

    public IReadOnlyList<ThumbnailData> Thumbnails => Memo.Cache(this, () =>
        Videos
            .FirstOrDefault()?
            .Thumbnails ??

        Array.Empty<ThumbnailData>()
    );

    public IReadOnlyList<PlaylistVideoData> Videos => Memo.Cache(this, () =>
        ContentRoot?
            .GetPropertyOrNull("contents")?
            .EnumerateArrayOrNull()?
            .Select(j => j.GetPropertyOrNull("playlistPanelVideoRenderer"))
            .WhereNotNull()
            .Select(j => new PlaylistVideoData(j))
            .ToArray() ??

        Array.Empty<PlaylistVideoData>()
    );

    public string? VisitorData => Memo.Cache(this, () =>
        _content
            .GetPropertyOrNull("responseContext")?
            .GetPropertyOrNull("visitorData")?
            .GetStringOrNull()
    );

    public PlaylistNextResponse(JsonElement content) => _content = content;
}

internal partial class PlaylistNextResponse
{
    public static PlaylistNextResponse Parse(string raw) => new(Json.Parse(raw));
}