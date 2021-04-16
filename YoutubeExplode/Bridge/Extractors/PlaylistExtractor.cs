using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using YoutubeExplode.Utils;
using YoutubeExplode.Utils.Extensions;

namespace YoutubeExplode.Bridge.Extractors
{
    internal partial class PlaylistExtractor
    {
        private readonly JsonElement _content;
        private readonly Memo _memo = new();

        public PlaylistExtractor(JsonElement content) => _content = content;

        private JsonElement? TryGetSidebar() => _memo.Wrap(() =>
            _content
                .GetPropertyOrNull("sidebar")?
                .GetPropertyOrNull("playlistSidebarRenderer")?
                .GetPropertyOrNull("items")
        );

        private JsonElement? TryGetSidebarPrimary() => _memo.Wrap(() =>
            TryGetSidebar()?
                .EnumerateArrayOrNull()?
                .ElementAtOrNull(0)?
                .GetPropertyOrNull("playlistSidebarPrimaryInfoRenderer")
        );

        private JsonElement? TryGetSidebarSecondary() => _memo.Wrap(() =>
            TryGetSidebar()?
                .EnumerateArrayOrNull()?
                .ElementAtOrNull(1)?
                .GetPropertyOrNull("playlistSidebarSecondaryInfoRenderer")
        );

        public bool IsPlaylistAvailable() => _memo.Wrap(() =>
            TryGetSidebar() is not null
        );

        public string? TryGetPlaylistTitle() => _memo.Wrap(() =>
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

        private JsonElement? TryGetPlaylistAuthorDetails() => _memo.Wrap(() =>
            TryGetSidebarSecondary()?
                .GetPropertyOrNull("videoOwner")?
                .GetPropertyOrNull("videoOwnerRenderer")
        );

        public string? TryGetPlaylistAuthor() => _memo.Wrap(() =>
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

        public string? TryGetPlaylistChannelId() => _memo.Wrap(() =>
            TryGetPlaylistAuthorDetails()?
                .GetPropertyOrNull("navigationEndpoint")?
                .GetPropertyOrNull("browseEndpoint")?
                .GetPropertyOrNull("browseId")?
                .GetStringOrNull()
        );

        public string? TryGetPlaylistDescription() => _memo.Wrap(() =>
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

        public IReadOnlyList<ThumbnailExtractor> GetPlaylistThumbnails() => _memo.Wrap(() =>
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

        private JsonElement? TryGetContentRoot() => _memo.Wrap(() =>
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

        public IReadOnlyList<PlaylistVideoExtractor> GetVideos() => _memo.Wrap(() =>
            TryGetContentRoot()?
                .EnumerateArrayOrNull()?
                .Select(j => j.GetPropertyOrNull("playlistVideoRenderer"))
                .WhereNotNull()
                .Select(j => new PlaylistVideoExtractor(j))
                .ToArray() ??

            Array.Empty<PlaylistVideoExtractor>()
        );

        public string? TryGetContinuationToken() => _memo.Wrap(() =>
            TryGetContentRoot()?
                .EnumerateArrayOrNull()?
                .Select(j => j.GetPropertyOrNull("continuationItemRenderer"))
                .WhereNotNull()
                .FirstOrNull()?
                .GetPropertyOrNull("continuationEndpoint")?
                .GetPropertyOrNull("continuationCommand")?
                .GetPropertyOrNull("token")?
                .GetStringOrNull()
        );
    }

    internal partial class PlaylistExtractor
    {
        public static PlaylistExtractor Create(string raw) => new(Json.Parse(raw));
    }
}