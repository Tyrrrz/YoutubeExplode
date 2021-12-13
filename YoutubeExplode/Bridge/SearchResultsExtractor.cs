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

    public SearchResultsExtractor(JsonElement content) => _content = content;

    // Search results response is incredibly inconsistent (5+ variations),
    // so we employ descendant searching, which is inefficient but resilient.
    private JsonElement? TryGetContentRoot() => Memo.Cache(this, () =>
        _content.GetPropertyOrNull("contents") ??
        _content.GetPropertyOrNull("onResponseReceivedCommands")
    );

    public IReadOnlyList<SearchResultVideoExtractor> GetVideos() => Memo.Cache(this, () =>
        TryGetContentRoot()?
            .EnumerateDescendantProperties("videoRenderer")
            .Select(j => new SearchResultVideoExtractor(j))
            .ToArray() ??

        Array.Empty<SearchResultVideoExtractor>()
    );

    public IReadOnlyList<SearchResultPlaylistExtractor> GetPlaylists() => Memo.Cache(this, () =>
        TryGetContentRoot()?
            .EnumerateDescendantProperties("playlistRenderer")
            .Select(j => new SearchResultPlaylistExtractor(j))
            .ToArray() ??

        Array.Empty<SearchResultPlaylistExtractor>()
    );

    public IReadOnlyList<SearchResultChannelExtractor> GetChannels() => Memo.Cache(this, () =>
        TryGetContentRoot()?
            .EnumerateDescendantProperties("channelRenderer")
            .Select(j => new SearchResultChannelExtractor(j))
            .ToArray() ??

        Array.Empty<SearchResultChannelExtractor>()
    );

    public string? TryGetContinuationToken() => Memo.Cache(this, () =>
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