using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using YoutubeExplode.Utils;
using YoutubeExplode.Utils.Extensions;

namespace YoutubeExplode.Bridge;

internal partial class SearchResponse
{
    private readonly JsonElement _content;

    // Search response is incredibly inconsistent (with at least 5 variations),
    // so we employ descendant searching, which is inefficient but resilient.

    private JsonElement? ContentRoot => Memo.Cache(this, () =>
        _content.GetPropertyOrNull("contents") ??
        _content.GetPropertyOrNull("onResponseReceivedCommands")
    );

    public IReadOnlyList<VideoData> Videos => Memo.Cache(this, () =>
        ContentRoot?
            .EnumerateDescendantProperties("videoRenderer")
            .Select(j => new VideoData(j))
            .ToArray() ??

        Array.Empty<VideoData>()
    );

    public IReadOnlyList<PlaylistData> Playlists => Memo.Cache(this, () =>
        ContentRoot?
            .EnumerateDescendantProperties("playlistRenderer")
            .Select(j => new PlaylistData(j))
            .ToArray() ??

        Array.Empty<PlaylistData>()
    );

    public IReadOnlyList<ChannelData> Channels => Memo.Cache(this, () =>
        ContentRoot?
            .EnumerateDescendantProperties("channelRenderer")
            .Select(j => new ChannelData(j))
            .ToArray() ??

        Array.Empty<ChannelData>()
    );

    public string? ContinuationToken => Memo.Cache(this, () =>
        ContentRoot?
            .EnumerateDescendantProperties("continuationCommand")
            .FirstOrNull()?
            .GetPropertyOrNull("token")?
            .GetStringOrNull()
    );

    public SearchResponse(JsonElement content) => _content = content;
}

internal partial class SearchResponse
{
    internal class VideoData
    {
        private readonly JsonElement _content;

        public string? Id => Memo.Cache(this, () =>
            _content
                .GetPropertyOrNull("videoId")?
                .GetStringOrNull()
        );

        public string? Title => Memo.Cache(this, () =>
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

        private JsonElement? AuthorDetails => Memo.Cache(this, () =>
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

        public string? Author => Memo.Cache(this, () =>
            AuthorDetails?
                .GetPropertyOrNull("text")?
                .GetStringOrNull()
        );

        public string? ChannelId => Memo.Cache(this, () =>
            AuthorDetails?
                .GetPropertyOrNull("navigationEndpoint")?
                .GetPropertyOrNull("browseEndpoint")?
                .GetPropertyOrNull("browseId")?
                .GetStringOrNull()
        );

        public TimeSpan? Duration => Memo.Cache(this, () =>
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
                .ParseTimeSpanOrNull(new[] { @"m\:ss", @"mm\:ss", @"h\:mm\:ss", @"hh\:mm\:ss" })
        );

        public IReadOnlyList<ThumbnailData> Thumbnails => Memo.Cache(this, () =>
            _content
                .GetPropertyOrNull("thumbnail")?
                .GetPropertyOrNull("thumbnails")?
                .EnumerateArrayOrNull()?
                .Select(j => new ThumbnailData(j))
                .ToArray() ??

            Array.Empty<ThumbnailData>()
        );

        public VideoData(JsonElement content) => _content = content;
    }
}

internal partial class SearchResponse
{
    public class PlaylistData
    {
        private readonly JsonElement _content;

        public string? Id => Memo.Cache(this, () =>
            _content
                .GetPropertyOrNull("playlistId")?
                .GetStringOrNull()
        );

        public string? Title => Memo.Cache(this, () =>
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

        private JsonElement? AuthorDetails => Memo.Cache(this, () =>
            _content
                .GetPropertyOrNull("longBylineText")?
                .GetPropertyOrNull("runs")?
                .EnumerateArrayOrNull()?
                .ElementAtOrNull(0)
        );

        public string? Author => Memo.Cache(this, () =>
            AuthorDetails?
                .GetPropertyOrNull("text")?
                .GetStringOrNull()
        );

        public string? ChannelId => Memo.Cache(this, () =>
            AuthorDetails?
                .GetPropertyOrNull("navigationEndpoint")?
                .GetPropertyOrNull("browseEndpoint")?
                .GetPropertyOrNull("browseId")?
                .GetStringOrNull()
        );

        public IReadOnlyList<ThumbnailData> Thumbnails => Memo.Cache(this, () =>
            _content
                .GetPropertyOrNull("thumbnails")?
                .EnumerateDescendantProperties("thumbnails")
                .SelectMany(j => j.EnumerateArrayOrEmpty())
                .Select(j => new ThumbnailData(j))
                .ToArray() ??

            Array.Empty<ThumbnailData>()
        );

        public PlaylistData(JsonElement content) => _content = content;
    }
}

internal partial class SearchResponse
{
    public class ChannelData
    {
        private readonly JsonElement _content;

        public string? Id => Memo.Cache(this, () =>
            _content
                .GetPropertyOrNull("channelId")?
                .GetStringOrNull()
        );

        public string? Title => Memo.Cache(this, () =>
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

        public IReadOnlyList<ThumbnailData> Thumbnails => Memo.Cache(this, () =>
            _content
                .GetPropertyOrNull("thumbnail")?
                .GetPropertyOrNull("thumbnails")?
                .EnumerateArrayOrNull()?
                .Select(j => new ThumbnailData(j))
                .ToArray() ??

            Array.Empty<ThumbnailData>()
        );

        public ChannelData(JsonElement content) => _content = content;
    }
}

internal partial class SearchResponse
{
    public static SearchResponse Parse(string raw) => new(Json.Parse(raw));
}