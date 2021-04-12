using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using YoutubeExplode.Utils;
using YoutubeExplode.Utils.Extensions;

namespace YoutubeExplode.Bridge.Extractors
{
    internal partial class SearchResultsExtractor
    {
        private readonly JsonElement _content;
        private readonly Memo _memo = new();

        public SearchResultsExtractor(JsonElement content) => _content = content;

        // Search results response is incredibly inconsistent (5+ variations),
        // so we employ descendent searching, which is inefficient but resilient.
        private JsonElement? TryGetContentRoot() => _memo.Wrap(() =>
            _content.GetPropertyOrNull("contents") ??
            _content.GetPropertyOrNull("onResponseReceivedCommands")
        );

        public string? TryGetContinuationToken() => _memo.Wrap(() =>
            TryGetContentRoot()?
                .EnumerateDescendantProperties("continuationCommand")
                .FirstOrNull()?
                .GetPropertyOrNull("token")?
                .GetStringOrNull()
        );

        public IReadOnlyList<SearchResultsVideoExtractor> GetVideos() => _memo.Wrap(() =>
            TryGetContentRoot()?
                .EnumerateDescendantProperties("videoRenderer")
                .Select(j => new SearchResultsVideoExtractor(j))
                .ToArray() ??

            Array.Empty<SearchResultsVideoExtractor>()
        );
    }

    internal partial class SearchResultsExtractor
    {
        public static SearchResultsExtractor Create(string raw) => new(Json.Parse(raw));
    }
}