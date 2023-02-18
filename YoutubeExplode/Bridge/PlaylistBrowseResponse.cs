using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using YoutubeExplode.Utils;
using YoutubeExplode.Utils.Extensions;

namespace YoutubeExplode.Bridge;

internal partial class PlaylistBrowseResponse : IPlaylistData
{
    private readonly JsonElement _content;

    private JsonElement? Sidebar => Memo.Cache(this, () =>
        _content
            .GetPropertyOrNull("sidebar")?
            .GetPropertyOrNull("playlistSidebarRenderer")?
            .GetPropertyOrNull("items")
    );

    private JsonElement? SidebarPrimary => Memo.Cache(this, () =>
        Sidebar?
            .EnumerateArrayOrNull()?
            .ElementAtOrNull(0)?
            .GetPropertyOrNull("playlistSidebarPrimaryInfoRenderer")
    );

    private JsonElement? SidebarSecondary => Memo.Cache(this, () =>
        Sidebar?
            .EnumerateArrayOrNull()?
            .ElementAtOrNull(1)?
            .GetPropertyOrNull("playlistSidebarSecondaryInfoRenderer")
    );

    public bool IsAvailable => Memo.Cache(this, () =>
        Sidebar is not null
    );

    public string? Title => Memo.Cache(this, () =>
        SidebarPrimary?
            .GetPropertyOrNull("title")?
            .GetPropertyOrNull("simpleText")?
            .GetStringOrNull() ??

        SidebarPrimary?
            .GetPropertyOrNull("title")?
            .GetPropertyOrNull("runs")?
            .EnumerateArrayOrNull()?
            .Select(j => j.GetPropertyOrNull("text")?.GetStringOrNull())
            .WhereNotNull()
            .ConcatToString()
    );

    private JsonElement? AuthorDetails => Memo.Cache(this, () =>
        SidebarSecondary?
            .GetPropertyOrNull("videoOwner")?
            .GetPropertyOrNull("videoOwnerRenderer")
    );

    public string? Author => Memo.Cache(this, () =>
        AuthorDetails?
            .GetPropertyOrNull("title")?
            .GetPropertyOrNull("simpleText")?
            .GetStringOrNull() ??

        AuthorDetails?
            .GetPropertyOrNull("title")?
            .GetPropertyOrNull("runs")?
            .EnumerateArrayOrNull()?
            .Select(j => j.GetPropertyOrNull("text")?.GetStringOrNull())
            .WhereNotNull()
            .ConcatToString()
    );

    public string? ChannelId => Memo.Cache(this, () =>
        AuthorDetails?
            .GetPropertyOrNull("navigationEndpoint")?
            .GetPropertyOrNull("browseEndpoint")?
            .GetPropertyOrNull("browseId")?
            .GetStringOrNull()
    );

    public string? Description => Memo.Cache(this, () =>
        SidebarPrimary?
            .GetPropertyOrNull("description")?
            .GetPropertyOrNull("simpleText")?
            .GetStringOrNull() ??

        SidebarPrimary?
            .GetPropertyOrNull("description")?
            .GetPropertyOrNull("runs")?
            .EnumerateArrayOrNull()?
            .Select(j => j.GetPropertyOrNull("text")?.GetStringOrNull())
            .WhereNotNull()
            .ConcatToString()
    );

    public IReadOnlyList<ThumbnailData> Thumbnails => Memo.Cache(this, () =>
        SidebarPrimary?
            .GetPropertyOrNull("thumbnailRenderer")?
            .GetPropertyOrNull("playlistVideoThumbnailRenderer")?
            .GetPropertyOrNull("thumbnail")?
            .GetPropertyOrNull("thumbnails")?
            .EnumerateArrayOrNull()?
            .Select(j => new ThumbnailData(j))
            .ToArray() ??

        SidebarPrimary?
            .GetPropertyOrNull("thumbnailRenderer")?
            .GetPropertyOrNull("playlistCustomThumbnailRenderer")?
            .GetPropertyOrNull("thumbnail")?
            .GetPropertyOrNull("thumbnails")?
            .EnumerateArrayOrNull()?
            .Select(j => new ThumbnailData(j))
            .ToArray() ??

        Array.Empty<ThumbnailData>()
    );

    public PlaylistBrowseResponse(JsonElement content) => _content = content;
}

internal partial class PlaylistBrowseResponse
{
    public static PlaylistBrowseResponse Parse(string raw) => new(Json.Parse(raw));
}