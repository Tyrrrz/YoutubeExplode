using System;
using System.Diagnostics.CodeAnalysis;

namespace YoutubeExplode.Videos.ClosedCaptions;

/// <summary>
/// Individual closed caption part contained within a track.
/// </summary>
public class ClosedCaptionPart
{
    /// <summary>
    /// Text displayed by the caption part.
    /// </summary>
    public string Text { get; }

    /// <summary>
    /// Time at which the caption part starts displaying, relative to the caption's own offset.
    /// </summary>
    public TimeSpan Offset { get; }

    /// <summary>
    /// Initializes an instance of <see cref="ClosedCaptionPart" />.
    /// </summary>
    public ClosedCaptionPart(string text, TimeSpan offset)
    {
        Text = text;
        Offset = offset;
    }

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public override string ToString() => Text;
}