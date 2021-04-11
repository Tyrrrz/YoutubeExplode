using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using YoutubeExplode.Utils;
using YoutubeExplode.Utils.Extensions;

namespace YoutubeExplode.Bridge.Extractors
{
    internal class SearchResultsVideoExtractor
    {
        private readonly JsonElement _content;
        private readonly Memo _memo = new();

        public SearchResultsVideoExtractor(JsonElement content) => _content = content;

        public string? TryGetVideoId() => _memo.Wrap(() =>
            _content
                .GetPropertyOrNull("videoId")?
                .GetStringOrNull()
        );

        public string? TryGetVideoTitle() => _memo.Wrap(() =>
            _content
                .GetPropertyOrNull("title")?
                .GetPropertyOrNull("runs")?
                .EnumerateArrayOrEmpty()
                .Select(j => j.GetPropertyOrNull("text")?.GetStringOrNull())
                .WhereNotNull()
                .ConcatToString()
        );

        private JsonElement? TryGetAuthorDetails() => _memo.Wrap(() =>
            _content
                .GetPropertyOrNull("longBylineText")?
                .GetPropertyOrNull("runs")?
                .EnumerateArrayOrEmpty()
                .ElementAtOrNull(0)
        );

        public string? TryGetVideoAuthor() => _memo.Wrap(() =>
            TryGetAuthorDetails()?
                .GetPropertyOrNull("text")?
                .GetStringOrNull()
        );

        public string? TryGetVideoChannelId() => _memo.Wrap(() =>
            TryGetAuthorDetails()?
                .GetPropertyOrNull("navigationEndpoint")?
                .GetPropertyOrNull("browseEndpoint")?
                .GetPropertyOrNull("browseId")?
                .GetStringOrNull()
        );

        public TimeSpan? TryGetVideoDuration() => _memo.Wrap(() =>
            _content
                .GetPropertyOrNull("lengthText")?
                .GetPropertyOrNull("simpleText")?
                .GetStringOrNull()?
                .ParseTimeSpanOrNull(new[] {@"m\:ss", @"mm\:ss", @"h\:mm\:ss", @"hh\:mm\:ss"})
        );

        public IReadOnlyList<SearchResultsVideoThumbnailExtractor> GetVideoThumbnails() => _memo.Wrap(() =>
            _content
                .GetPropertyOrNull("thumbnail")?
                .GetPropertyOrNull("thumbnails")?
                .EnumerateArrayOrEmpty()
                .Select(j => new SearchResultsVideoThumbnailExtractor(j))
                .ToArray() ??

            Array.Empty<SearchResultsVideoThumbnailExtractor>()
        );

        public string? TryGetVideoDescription() => _memo.Wrap(() =>
            _content
                .GetPropertyOrNull("descriptionSnippet")?
                .GetPropertyOrNull("runs")?
                .EnumerateArrayOrEmpty()
                .Select(j => j.GetPropertyOrNull("text")?.GetStringOrNull())
                .WhereNotNull()
                .ConcatToString()
        );

        public long? TryGetVideoViewCount() => _memo.Wrap(() =>
            _content
                .GetPropertyOrNull("viewCountText")?
                .GetPropertyOrNull("simpleText")?
                .GetStringOrNull()?
                .StripNonDigit()
                .ParseLongOrNull()
        );
    }
}