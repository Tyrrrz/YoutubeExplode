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
            .GetPropertyOrNull("playlist") ??

        _content
            .GetPropertyOrNull("continuationContents")?
            .GetPropertyOrNull("playlistPanelContinuation")


    );
    private bool? TryGetPlaylistInfiniteProperty() => Memo.Cache(this, () =>
        TryGetPlaylistProperty()?
            .GetPropertyOrNull("isInfinite")?
            .GetBoolean()
    );

    private JsonElement? TryGetPlaylistContinuationProperty() => Memo.Cache(this, () =>
        _content
            .GetPropertyOrNull("continuationContents")?
            .GetPropertyOrNull("playlistPanelContinuation")?
            .GetPropertyOrNull("contents")
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

    public bool IsMixPlaylist() => Memo.Cache(this, () =>
        TryGetPlaylistInfiniteProperty() is not false
    );

    public string? TryGetPlaylistTitle() => Memo.Cache(this, () =>
         // next endpoint part
        TryGetPlaylistProperty()?
            .GetPropertyOrNull("playlist")?
            .GetPropertyOrNull("title")?
            .GetStringOrNull() ??
        // browse endpoint part
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
        TryGetSidebarPrimary()?
            .GetPropertyOrNull("thumbnailRenderer")?
            .GetPropertyOrNull("playlistVideoThumbnailRenderer")?
            .GetPropertyOrNull("thumbnail")?
            .GetPropertyOrNull("thumbnails")?
            .EnumerateArrayOrNull()?
            .Select(j => new ThumbnailExtractor(j))
            .ToArray() ??

        Array.Empty<ThumbnailExtractor>()
    );

    private JsonElement? TryGetContentRoot() => Memo.Cache(this, () =>
        // Initial response
        _content
            .GetPropertyOrNull("contents")?
            .GetPropertyOrNull("twoColumnBrowseResultsRenderer")?
            .GetPropertyOrNull("tabs")?
            .EnumerateArrayOrNull()?
            .ElementAtOrNull(0)?
            .GetPropertyOrNull("tabRenderer")?
            .GetPropertyOrNull("content")?
            .GetPropertyOrNull("sectionListRenderer")?
            .GetPropertyOrNull("contents")?
            .EnumerateArrayOrNull()?
            .ElementAtOrNull(0)?
            .GetPropertyOrNull("itemSectionRenderer")?
            .GetPropertyOrNull("contents")?
            .EnumerateArrayOrNull()?
            .ElementAtOrNull(0)?
            .GetPropertyOrNull("playlistVideoListRenderer")?
            .GetPropertyOrNull("contents") ??

        // Continuation response
        _content
            .GetPropertyOrNull("onResponseReceivedActions")?
            .EnumerateArrayOrNull()?
            .ElementAtOrNull(0)?
            .GetPropertyOrNull("appendContinuationItemsAction")?
            .GetPropertyOrNull("continuationItems")
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

    public string? TryGetContinuationToken() => Memo.Cache(this, () =>
        TryGetPlaylistProperty()?
            .GetPropertyOrNull("continuations")?
            .EnumerateArrayOrNull()?
            .Select(j => j.GetPropertyOrNull("nextContinuationData"))
            .WhereNotNull()
            .FirstOrDefault()
            .GetPropertyOrNull("continuation")?
            .GetStringOrNull()
    );
}

internal partial class PlaylistExtractor
{
    public static PlaylistExtractor Create(string raw) => new(Json.Parse(raw));
}