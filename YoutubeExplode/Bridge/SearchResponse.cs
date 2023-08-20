using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Lazy;
using YoutubeExplode.Utils;
using YoutubeExplode.Utils.Extensions;

namespace YoutubeExplode.Bridge;

internal partial class SearchResponse
{
    private readonly JsonElement _content;

    // Search response is incredibly inconsistent (with at least 5 variations),
    // so we employ descendant searching, which is inefficient but resilient.

    [Lazy]
    private JsonElement? ContentRoot =>
        _content.GetPropertyOrNull("contents") ??
        _content.GetPropertyOrNull("onResponseReceivedCommands");

    [Lazy]
    public IReadOnlyList<VideoData> Videos => ContentRoot?
        .EnumerateDescendantProperties("videoRenderer")
        .Select(j => new VideoData(j))
        .ToArray() ?? Array.Empty<VideoData>();

    [Lazy]
    public IReadOnlyList<PlaylistData> Playlists => ContentRoot?
        .EnumerateDescendantProperties("playlistRenderer")
        .Select(j => new PlaylistData(j))
        .ToArray() ?? Array.Empty<PlaylistData>();

    [Lazy]
    public IReadOnlyList<ChannelData> Channels => ContentRoot?
        .EnumerateDescendantProperties("channelRenderer")
        .Select(j => new ChannelData(j))
        .ToArray() ?? Array.Empty<ChannelData>();

    [Lazy]
    public string? ContinuationToken => ContentRoot?
        .EnumerateDescendantProperties("continuationCommand")
        .FirstOrNull()?
        .GetPropertyOrNull("token")?
        .GetStringOrNull();

    public SearchResponse(JsonElement content) => _content = content;
}

internal partial class SearchResponse
{
    internal class VideoData
    {
        private readonly JsonElement _content;

        [Lazy]
        public string? Id => _content.GetPropertyOrNull("videoId")?.GetStringOrNull();

        [Lazy]
        public string? Title =>
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
                .ConcatToString();

        [Lazy]
        private JsonElement? AuthorDetails =>
            _content
                .GetPropertyOrNull("longBylineText")?
                .GetPropertyOrNull("runs")?
                .EnumerateArrayOrNull()?
                .ElementAtOrNull(0) ??

            _content
                .GetPropertyOrNull("shortBylineText")?
                .GetPropertyOrNull("runs")?
                .EnumerateArrayOrNull()?
                .ElementAtOrNull(0);

        [Lazy]
        public string? Author => AuthorDetails?.GetPropertyOrNull("text")?.GetStringOrNull();

        [Lazy]
        public string? ChannelId => AuthorDetails?
            .GetPropertyOrNull("navigationEndpoint")?
            .GetPropertyOrNull("browseEndpoint")?
            .GetPropertyOrNull("browseId")?
            .GetStringOrNull();

        [Lazy]
        public TimeSpan? Duration =>
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
                .ParseTimeSpanOrNull(new[] { @"m\:ss", @"mm\:ss", @"h\:mm\:ss", @"hh\:mm\:ss" });

        [Lazy]
        public IReadOnlyList<ThumbnailData> Thumbnails => _content
            .GetPropertyOrNull("thumbnail")?
            .GetPropertyOrNull("thumbnails")?
            .EnumerateArrayOrNull()?
            .Select(j => new ThumbnailData(j))
            .ToArray() ?? Array.Empty<ThumbnailData>();

        public VideoData(JsonElement content) => _content = content;
    }
}

internal partial class SearchResponse
{
    public class PlaylistData
    {
        private readonly JsonElement _content;

        [Lazy]
        public string? Id => _content.GetPropertyOrNull("playlistId")?.GetStringOrNull();

        [Lazy]
        public string? Title =>
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
                .ConcatToString();

        [Lazy]
        private JsonElement? AuthorDetails => _content
            .GetPropertyOrNull("longBylineText")?
            .GetPropertyOrNull("runs")?
            .EnumerateArrayOrNull()?
            .ElementAtOrNull(0);

        [Lazy]
        public string? Author => AuthorDetails?.GetPropertyOrNull("text")?.GetStringOrNull();

        [Lazy]
        public string? ChannelId => AuthorDetails?
            .GetPropertyOrNull("navigationEndpoint")?
            .GetPropertyOrNull("browseEndpoint")?
            .GetPropertyOrNull("browseId")?
            .GetStringOrNull();

        [Lazy]
        public IReadOnlyList<ThumbnailData> Thumbnails => _content
            .GetPropertyOrNull("thumbnails")?
            .EnumerateDescendantProperties("thumbnails")
            .SelectMany(j => j.EnumerateArrayOrEmpty())
            .Select(j => new ThumbnailData(j))
            .ToArray() ?? Array.Empty<ThumbnailData>();

        public PlaylistData(JsonElement content) => _content = content;
    }
}

internal partial class SearchResponse
{
    public class ChannelData
    {
        private readonly JsonElement _content;

        [Lazy]
        public string? Id => _content.GetPropertyOrNull("channelId")?.GetStringOrNull();

        [Lazy]
        public string? Title =>
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
                .ConcatToString();

        [Lazy]
        public IReadOnlyList<ThumbnailData> Thumbnails => _content
            .GetPropertyOrNull("thumbnail")?
            .GetPropertyOrNull("thumbnails")?
            .EnumerateArrayOrNull()?
            .Select(j => new ThumbnailData(j))
            .ToArray() ?? Array.Empty<ThumbnailData>();

        public ChannelData(JsonElement content) => _content = content;
    }
}

internal partial class SearchResponse
{
    public static SearchResponse Parse(string raw) => new(Json.Parse(raw));
}