using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Lazy;
using YoutubeExplode.Utils;
using YoutubeExplode.Utils.Extensions;

namespace YoutubeExplode.Bridge;

internal partial class PlaylistNextResponse : IPlaylistData
{
    private readonly JsonElement _content;

    [Lazy]
    private JsonElement? ContentRoot => _content
        .GetPropertyOrNull("contents")?
        .GetPropertyOrNull("twoColumnWatchNextResults")?
        .GetPropertyOrNull("playlist")?
        .GetPropertyOrNull("playlist");

    [Lazy]
    public bool IsAvailable => ContentRoot is not null;

    [Lazy]
    public string? Title => ContentRoot?.GetPropertyOrNull("title")?.GetStringOrNull();

    [Lazy]
    public string? Author => ContentRoot?
        .GetPropertyOrNull("ownerName")?
        .GetPropertyOrNull("simpleText")?
        .GetStringOrNull();

    public string? ChannelId => null;

    public string? Description => null;

    [Lazy]
    public IReadOnlyList<ThumbnailData> Thumbnails =>
        Videos.FirstOrDefault()?.Thumbnails ??
        Array.Empty<ThumbnailData>();

    [Lazy]
    public IReadOnlyList<PlaylistVideoData> Videos => ContentRoot?
        .GetPropertyOrNull("contents")?
        .EnumerateArrayOrNull()?
        .Select(j => j.GetPropertyOrNull("playlistPanelVideoRenderer"))
        .WhereNotNull()
        .Select(j => new PlaylistVideoData(j))
        .ToArray() ?? Array.Empty<PlaylistVideoData>();

    [Lazy]
    public string? VisitorData => _content
        .GetPropertyOrNull("responseContext")?
        .GetPropertyOrNull("visitorData")?
        .GetStringOrNull();

    public PlaylistNextResponse(JsonElement content) => _content = content;
}

internal partial class PlaylistNextResponse
{
    public static PlaylistNextResponse Parse(string raw) => new(Json.Parse(raw));
}