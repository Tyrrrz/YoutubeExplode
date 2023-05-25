using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using YoutubeExplode.Videos;

namespace YoutubeExplode.Common;

/// <summary>
/// Thumbnail image.
/// </summary>
public partial class Thumbnail
{
    /// <summary>
    /// Thumbnail URL.
    /// </summary>
    public string Url { get; }

    /// <summary>
    /// Thumbnail resolution.
    /// </summary>
    public Resolution Resolution { get; }

    /// <summary>
    /// Initializes an instance of <see cref="Thumbnail" />.
    /// </summary>
    public Thumbnail(string url, Resolution resolution)
    {
        Url = url;
        Resolution = resolution;
    }

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public override string ToString() => $"Thumbnail ({Resolution})";
}

public partial class Thumbnail
{
    internal static IReadOnlyList<Thumbnail> GetDefaultSet(VideoId videoId) => new[]
    {
        new Thumbnail($"https://img.youtube.com/vi/{videoId}/default.jpg", new Resolution(120, 90)),
        new Thumbnail($"https://img.youtube.com/vi/{videoId}/mqdefault.jpg", new Resolution(320, 180)),
        new Thumbnail($"https://img.youtube.com/vi/{videoId}/hqdefault.jpg", new Resolution(480, 360))
    };
}

/// <summary>
/// Extensions for <see cref="Thumbnail" />.
/// </summary>
public static class ThumbnailExtensions
{
    /// <summary>
    /// Gets the thumbnail with the highest resolution (by area).
    /// Returns null if the sequence is empty.
    /// </summary>
    public static Thumbnail? TryGetWithHighestResolution(this IEnumerable<Thumbnail> thumbnails) =>
        thumbnails.MaxBy(t => t.Resolution.Area);

    /// <summary>
    /// Gets the thumbnail with the highest resolution (by area).
    /// </summary>
    public static Thumbnail GetWithHighestResolution(this IEnumerable<Thumbnail> thumbnails) =>
        thumbnails.TryGetWithHighestResolution() ??
        throw new InvalidOperationException("Input thumbnail collection is empty.");
}