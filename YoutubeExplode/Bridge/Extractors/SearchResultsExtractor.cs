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

        private JsonElement? TryGetContents() => _memo.Wrap(() =>
            _content
                .GetPropertyOrNull("contents")?
                .GetPropertyOrNull("twoColumnSearchResultsRenderer")?
                .GetPropertyOrNull("primaryContents")?
                .GetPropertyOrNull("sectionListRenderer")?
                .GetPropertyOrNull("contents")
        );

        public string? TryGetContinuationToken() => _memo.Wrap(() =>
            TryGetContents()?
                .EnumerateArrayOrEmpty()
                .ElementAtOrNull(1)?
                .GetPropertyOrNull("continuationItemRenderer")?
                .GetPropertyOrNull("continuationEndpoint")?
                .GetPropertyOrNull("continuationCommand")?
                .GetPropertyOrNull("token")?
                .GetStringOrNull()
        );

        public IReadOnlyList<SearchResultsVideoExtractor> GetVideos() => _memo.Wrap(() =>
            TryGetContents()?
                .EnumerateArrayOrEmpty()
                .ElementAtOrNull(0)?
                .GetPropertyOrNull("itemSectionRenderer")?
                .GetPropertyOrNull("contents")?
                .EnumerateArrayOrEmpty()
                .Select(j => j.GetPropertyOrNull("videoRenderer"))
                .WhereNotNull()
                .Where(j => j.GetPropertyOrNull("videoId") is not null)
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