using System.Text.Json;
using YoutubeExplode.Utils;
using YoutubeExplode.Utils.Extensions;

namespace YoutubeExplode.Bridge;

internal class HeatmarkerExtractor
{
    private readonly JsonElement _content;

    public HeatmarkerExtractor(JsonElement content) => _content = content;

    public long? TryGetTimeRangeStartMillis() => Memo.Cache(this, () =>
        _content.GetPropertyOrNull("heatMarkerRenderer")?.GetPropertyOrNull("timeRangeStartMillis")?.GetInt64OrNull()
    );

    public long? TryGetMarkerRangeStartMillis() => Memo.Cache(this, () =>
        _content.GetPropertyOrNull("heatMarkerRenderer")?.GetPropertyOrNull("timeRangeStartMillis")?.GetInt64OrNull()
    );

    public decimal? TryGetHeatMarkerIntensityScoreNormalized() => Memo.Cache(this, () =>
        _content.GetPropertyOrNull("heatMarkerRenderer")?.GetPropertyOrNull("heatMarkerIntensityScoreNormalized")?.GetDecimal()
    );
}
