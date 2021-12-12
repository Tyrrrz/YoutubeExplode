using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using YoutubeExplode.Utils;
using YoutubeExplode.Utils.Extensions;

namespace YoutubeExplode.Bridge;

internal partial class SearchResultsExtractor
{
    private readonly JsonElement _content;
    private readonly Memo _memo = new();

    public SearchResultsExtractor(JsonElement content) => _content = content;

    // Search results response is incredibly inconsistent (5+ variations),
    // so we employ descendant searching, which is inefficient but resilient.
    private JsonElement? TryGetContentRoot() => _memo.Wrap(() =>
        _content.GetPropertyOrNull("contents") ??
        _content.GetPropertyOrNull("onResponseReceivedCommands")
    );

    public IReadOnlyList<SearchResultVideoExtractor> GetVideos() => _memo.Wrap(() =>
        TryGetContentRoot()?
            .EnumerateDescendantProperties("videoRenderer")
            .Select(j => new SearchResultVideoExtractor(j))
            .ToArray() ??

        Array.Empty<SearchResultVideoExtractor>()
    );

    public IReadOnlyList<SearchResultPlaylistExtractor> GetPlaylists() => _memo.Wrap(() =>
        TryGetContentRoot()?
            .EnumerateDescendantProperties("playlistRenderer")
            .Select(j => new SearchResultPlaylistExtractor(j))
            .ToArray() ??

        Array.Empty<SearchResultPlaylistExtractor>()
    );

    public IReadOnlyList<SearchResultChannelExtractor> GetChannels() => _memo.Wrap(() =>
        TryGetContentRoot()?
            .EnumerateDescendantProperties("channelRenderer")
            .Select(j => new SearchResultChannelExtractor(j))
            .ToArray() ??

        Array.Empty<SearchResultChannelExtractor>()
    );

    public string? TryGetContinuationToken() => _memo.Wrap(() =>
        TryGetContentRoot()?
            .EnumerateDescendantProperties("continuationCommand")
            .FirstOrNull()?
            .GetPropertyOrNull("token")?
            .GetStringOrNull()
    );
}

internal partial class SearchResultsExtractor
{
    public static SearchResultsExtractor Create(string raw) => new(Json.Parse(raw));
}