using System.Text.Json;
using Lazy;
using YoutubeExplode.Utils.Extensions;

namespace YoutubeExplode.Bridge;

internal class ThumbnailData
{
    private readonly JsonElement _content;

    public ThumbnailData(JsonElement content) => _content = content;

    [Lazy]
    public string? Url => _content.GetPropertyOrNull("url")?.GetStringOrNull();

    [Lazy]
    public int? Width => _content.GetPropertyOrNull("width")?.GetInt32OrNull();

    [Lazy]
    public int? Height => _content.GetPropertyOrNull("height")?.GetInt32OrNull();
}