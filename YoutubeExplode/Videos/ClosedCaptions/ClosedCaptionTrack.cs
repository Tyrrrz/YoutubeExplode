using System;
using System.Collections.Generic;
using System.Linq;

namespace YoutubeExplode.Videos.ClosedCaptions;

/// <summary>
/// Contains closed captions in a specific language.
/// </summary>
public class ClosedCaptionTrack
{
    /// <summary>
    /// Closed captions included in the track.
    /// </summary>
    public IReadOnlyList<ClosedCaption> Captions { get; }

    /// <summary>
    /// Initializes an instance of <see cref="ClosedCaptionTrack" />.
    /// </summary>
    public ClosedCaptionTrack(IReadOnlyList<ClosedCaption> captions)
    {
        Captions = captions;
    }

    /// <summary>
    /// Gets the caption displayed at the specified point in time.
    /// Returns null if not found.
    /// </summary>
    public ClosedCaption? TryGetByTime(TimeSpan time) =>
        Captions.FirstOrDefault(c => time >= c.Offset && time <= c.Offset + c.Duration);

    /// <summary>
    /// Gets the caption displayed at the specified point in time.
    /// </summary>
    public ClosedCaption GetByTime(TimeSpan time) =>
        TryGetByTime(time) ??
        throw new InvalidOperationException($"No closed caption found at {time}.");
}