using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using YoutubeExplode.Utils;
using YoutubeExplode.Utils.Extensions;

namespace YoutubeExplode.Bridge.Extractors
{
    internal class SearchResultChannelExtractor
    {
        private readonly JsonElement _content;
        private readonly Memo _memo = new();

        public SearchResultChannelExtractor(JsonElement content) =>
            _content = content;

        public string? TryGetChannelId() => _memo.Wrap(() =>
            _content
                .GetPropertyOrNull("channelId")?
                .GetStringOrNull()
        );

        public string? TryGetChannelTitle() => _memo.Wrap(() =>
            _content
                .GetPropertyOrNull("title")?
                .GetPropertyOrNull("simpleText")?
                .GetStringOrNull() ??

            _content
                .GetPropertyOrNull("title")?
                .GetPropertyOrNull("runs")?
                .EnumerateArrayOrEmpty()
                .Select(j => j.GetPropertyOrNull("text")?.GetStringOrNull())
                .WhereNotNull()
                .ConcatToString()
        );

        public IReadOnlyList<ThumbnailExtractor> GetChannelThumbnails() => _memo.Wrap(() =>
            _content
                .GetPropertyOrNull("thumbnail")?
                .GetPropertyOrNull("thumbnails")?
                .EnumerateArrayOrEmpty()
                .Select(j => new ThumbnailExtractor(j))
                .ToArray() ??

            Array.Empty<ThumbnailExtractor>()
        );
    }
}