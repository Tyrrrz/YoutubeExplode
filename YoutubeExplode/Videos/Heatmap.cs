using System.Diagnostics.CodeAnalysis;

namespace YoutubeExplode.Videos;

/// <summary>
/// Heatmap statistics.
/// </summary>
public class Heatmap
{
    /// <summary>
    /// Time Range Starting in Milliseconds.
    /// </summary>
    public long TimeRangeStartMillis { get; }

    /// <summary>
    /// Marker Duration starting in Milliseconds.
    /// </summary>
    public long MarkerDurationMillis { get; }

    /// <summary>
    /// Heat Marker Insensity Score, normalized.
    /// Range from 0 to 1.
    /// </summary>
    public decimal HeatMarkerIntensityScoreNormalized { get; }

    /// <summary>
    /// Initializes an instance of <see cref="Heatmap" />.
    /// </summary>
    public Heatmap(long timeRangeStartMillis, long markerDurationMillis, decimal heatMarkerIntensityScoreNormalized)
    {
        TimeRangeStartMillis = timeRangeStartMillis;
        MarkerDurationMillis = markerDurationMillis;
        HeatMarkerIntensityScoreNormalized = heatMarkerIntensityScoreNormalized;
    }

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public override string ToString() => $"Heatmap: {TimeRangeStartMillis},{MarkerDurationMillis},{HeatMarkerIntensityScoreNormalized}";
}
