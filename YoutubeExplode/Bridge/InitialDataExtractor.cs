using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using YoutubeExplode.Utils;
using YoutubeExplode.Utils.Extensions;

namespace YoutubeExplode.Bridge;

internal partial class InitialDataExtractor
{
    private readonly JsonElement _content;

    public InitialDataExtractor(JsonElement content) => _content = content;

    public IReadOnlyList<HeatmarkerExtractor>? TryGetHeatmap() => Memo.Cache(this, () =>
        _content
            .GetPropertyOrNull("playerOverlays")?
            .GetPropertyOrNull("playerOverlayRenderer")?
            .GetPropertyOrNull("decoratedPlayerBarRenderer")?
            .GetPropertyOrNull("decoratedPlayerBarRenderer")?
            .GetPropertyOrNull("playerBar")?
            .GetPropertyOrNull("multiMarkersPlayerBarRenderer")?
            .GetPropertyOrNull("markersMap")?
            .EnumerateArrayOrNull()?
            .FirstOrNull()?
            .GetPropertyOrNull("value")?
            .GetPropertyOrNull("heatmap")?
            .GetPropertyOrNull("heatmapRenderer")?
            .GetPropertyOrNull("heatMarkers")?
            .EnumerateArrayOrNull()?
            .Select(j => new HeatmarkerExtractor(j))
            .ToArray() ??
        Array.Empty<HeatmarkerExtractor>()
    );
}
