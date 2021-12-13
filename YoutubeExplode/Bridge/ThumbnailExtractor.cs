using System.Text.Json;
using YoutubeExplode.Utils;
using YoutubeExplode.Utils.Extensions;

namespace YoutubeExplode.Bridge;

internal class ThumbnailExtractor
{
    private readonly JsonElement _content;

    public ThumbnailExtractor(JsonElement content) => _content = content;

    public string? TryGetUrl() => Memo.Cache(this, () =>
        _content.GetPropertyOrNull("url")?.GetStringOrNull()
    );

    public int? TryGetWidth() => Memo.Cache(this, () =>
        _content.GetPropertyOrNull("width")?.GetInt32OrNull()
    );

    public int? TryGetHeight() => Memo.Cache(this, () =>
        _content.GetPropertyOrNull("height")?.GetInt32OrNull()
    );
}