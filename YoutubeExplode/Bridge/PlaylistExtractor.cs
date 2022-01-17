using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using YoutubeExplode.Utils;
using YoutubeExplode.Utils.Extensions;

namespace YoutubeExplode.Bridge;

internal partial class PlaylistExtractor
{
    private readonly JsonElement _content;

    public PlaylistExtractor(JsonElement content) => _content = content;

    private JsonElement? TryGetSidebar() => Memo.Cache(this, () =>
        _content
            .GetPropertyOrNull("sidebar")?
            .GetPropertyOrNull("playlistSidebarRenderer")?
            .GetPropertyOrNull("items")
    );

    private JsonElement? TryGetPlaylistProperty() => Memo.Cache(this, () =>
        _content
            .GetPropertyOrNull("contents")?
            .GetPropertyOrNull("twoColumnWatchNextResults")?
            .GetPropertyOrNull("playlist")?
            .GetPropertyOrNull("playlist") 

    );

    private JsonElement? TryGetSidebarPrimary() => Memo.Cache(this, () =>
        TryGetSidebar()?
            .EnumerateArrayOrNull()?
            .ElementAtOrNull(0)?
            .GetPropertyOrNull("playlistSidebarPrimaryInfoRenderer")
    );

    private JsonElement? TryGetSidebarSecondary() => Memo.Cache(this, () =>
        TryGetSidebar()?
            .EnumerateArrayOrNull()?
            .ElementAtOrNull(1)?
            .GetPropertyOrNull("playlistSidebarSecondaryInfoRenderer")
    );

    public bool IsPlaylistVideosAvailable() => Memo.Cache(this, () =>
        TryGetPlaylistProperty() is not null
    );

    public bool IsPlaylistDetailsAvailable() => Memo.Cache(this, () =>
        TryGetSidebar() is not null
    );

    public string? TryGetPlaylistTitle() => Memo.Cache(this, () =>
        // next endpoint part
        TryGetPlaylistProperty()?
            .GetPropertyOrNull("title")?
            .GetStringOrNull() ??

        // browse endpoint part
        TryGetSidebarPrimary()?
            .GetPropertyOrNull("title")?
            .GetPropertyOrNull("simpleText")?
            .GetStringOrNull() ??

        TryGetSidebarPrimary()?
            .GetPropertyOrNull("title")?
            .GetPropertyOrNull("runs")?
            .EnumerateArrayOrNull()?
            .Select(j => j.GetPropertyOrNull("text")?.GetStringOrNull())
            .WhereNotNull()
            .ConcatToString()
    );

    private JsonElement? TryGetPlaylistAuthorDetails() => Memo.Cache(this, () =>
        TryGetSidebarSecondary()?
            .GetPropertyOrNull("videoOwner")?
            .GetPropertyOrNull("videoOwnerRenderer")


    );

    public string? TryGetPlaylistAuthor() => Memo.Cache(this, () =>
        //next enpoint part
        TryGetPlaylistProperty()?
            .GetPropertyOrNull("ownerName")?
            .GetPropertyOrNull("simpleText")?
            .GetStringOrNull() ??

        //browse endpoint part
        TryGetPlaylistAuthorDetails()?
            .GetPropertyOrNull("title")?
            .GetPropertyOrNull("simpleText")?
            .GetStringOrNull() ??

        TryGetPlaylistAuthorDetails()?
            .GetPropertyOrNull("title")?
            .GetPropertyOrNull("runs")?
            .EnumerateArrayOrNull()?
            .Select(j => j.GetPropertyOrNull("text")?.GetStringOrNull())
            .WhereNotNull()
            .ConcatToString()
    );

    public string? TryGetPlaylistChannelId() => Memo.Cache(this, () =>
        TryGetPlaylistAuthorDetails()?
            .GetPropertyOrNull("navigationEndpoint")?
            .GetPropertyOrNull("browseEndpoint")?
            .GetPropertyOrNull("browseId")?
            .GetStringOrNull()
    );

    public string? TryGetPlaylistDescription() => Memo.Cache(this, () =>
        TryGetSidebarPrimary()?
            .GetPropertyOrNull("description")?
            .GetPropertyOrNull("simpleText")?
            .GetStringOrNull() ??

        TryGetSidebarPrimary()?
            .GetPropertyOrNull("description")?
            .GetPropertyOrNull("runs")?
            .EnumerateArrayOrNull()?
            .Select(j => j.GetPropertyOrNull("text")?.GetStringOrNull())
            .WhereNotNull()
            .ConcatToString()
    );

    public IReadOnlyList<ThumbnailExtractor> GetPlaylistThumbnails() => Memo.Cache(this, () =>
        //next endpoint part
        GetVideos()
            .FirstOrDefault()?
            .GetVideoThumbnails()
            .ToArray() ??

        //browse endpoint part
        TryGetSidebarPrimary()?
            .GetPropertyOrNull("thumbnailRenderer")?
            .GetPropertyOrNull("playlistVideoThumbnailRenderer")?
            .GetPropertyOrNull("thumbnail")?
            .GetPropertyOrNull("thumbnails")?
            .EnumerateArrayOrNull()?
            .Select(j => new ThumbnailExtractor(j))
            .ToArray() ??
        
        TryGetSidebarPrimary()?
            .GetPropertyOrNull("thumbnailRenderer")?
            .GetPropertyOrNull("playlistCustomThumbnailRenderer")?
            .GetPropertyOrNull("thumbnail")?
            .GetPropertyOrNull("thumbnails")?
            .EnumerateArrayOrNull()?
            .Select(j => new ThumbnailExtractor(j))
            .ToArray() ??

        Array.Empty<ThumbnailExtractor>()
    );

    public string? TryGetPlayListUrl() => Memo.Cache(this, () =>
     TryGetPlaylistProperty()?
         .GetPropertyOrNull("playlistShareUrl")?
         .GetStringOrNull()
    );

    public IReadOnlyList<PlaylistVideoExtractor> GetVideos() => Memo.Cache(this, () =>
        TryGetPlaylistProperty()?
            .GetPropertyOrNull("contents")?
            .EnumerateArrayOrNull()?
            .Select(j => j.GetPropertyOrNull("playlistPanelVideoRenderer"))
            .WhereNotNull()
            .Select(j => new PlaylistVideoExtractor(j))
            .ToArray() ??

        Array.Empty<PlaylistVideoExtractor>()
    );

    public string? TryGetLastVideoId() => Memo.Cache(this, () =>
        GetVideos()?
            .LastOrDefault()?
            .TryGetVideoId()
    );

    public int? TryGetLastIndex() => Memo.Cache(this, () =>
        GetVideos()
            .LastOrDefault()?
            .TryGetIndex()
    );

    public string? TryGetVisitorData() => Memo.Cache(this, () =>
        _content
            .GetPropertyOrNull("responseContext")?
            .GetPropertyOrNull("visitorData")?
            .GetStringOrNull()
    );
}

internal partial class PlaylistExtractor
{
    public static PlaylistExtractor Create(string raw) => new(Json.Parse(raw));
}