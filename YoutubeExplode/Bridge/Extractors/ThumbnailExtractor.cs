using System.Text.Json;
using YoutubeExplode.Utils;
using YoutubeExplode.Utils.Extensions;

namespace YoutubeExplode.Bridge.Extractors
{
    internal class ThumbnailExtractor
    {
        private readonly JsonElement _content;
        private readonly Memo _memo = new();

        public ThumbnailExtractor(JsonElement content) => _content = content;

        public string? TryGetUrl() => _memo.Wrap(() =>
            _content.GetPropertyOrNull("url")?.GetStringOrNull()
        );

        public int? TryGetWidth() => _memo.Wrap(() =>
            _content.GetPropertyOrNull("width")?.GetInt32OrNull()
        );

        public int? TryGetHeight() => _memo.Wrap(() =>
            _content.GetPropertyOrNull("height")?.GetInt32OrNull()
        );
    }
}