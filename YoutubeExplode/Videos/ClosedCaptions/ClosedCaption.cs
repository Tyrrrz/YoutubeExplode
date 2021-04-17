using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace YoutubeExplode.Videos.ClosedCaptions
{
    /// <summary>
    /// Individual closed caption contained within a track.
    /// </summary>
    public class ClosedCaption
    {
        /// <summary>
        /// Text displayed by the caption.
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// Time at which the caption starts being displayed.
        /// </summary>
        public TimeSpan Offset { get; }

        /// <summary>
        /// Duration of time for which the caption is displayed.
        /// </summary>
        public TimeSpan Duration { get; }

        /// <summary>
        /// Caption parts (usually representing individual words).
        /// </summary>
        /// <remarks>
        /// May be empty because not all captions have parts.
        /// </remarks>
        public IReadOnlyList<ClosedCaptionPart> Parts { get; }

        /// <summary>
        /// Initializes an instance of <see cref="ClosedCaption"/>.
        /// </summary>
        public ClosedCaption(
            string text,
            TimeSpan offset,
            TimeSpan duration,
            IReadOnlyList<ClosedCaptionPart> parts)
        {
            Text = text;
            Offset = offset;
            Duration = duration;
            Parts = parts;
        }

        /// <summary>
        /// Gets the caption part displayed at the specified point in time, relative to the caption's own offset.
        /// Returns null if not found.
        /// </summary>
        public ClosedCaptionPart? TryGetPartByTime(TimeSpan time) =>
            Parts.FirstOrDefault(p => p.Offset >= time);

        /// <summary>
        /// Gets the caption part displayed at the specified point in time, relative to the caption's own offset.
        /// </summary>
        public ClosedCaptionPart GetPartByTime(TimeSpan time) =>
            TryGetPartByTime(time) ??
            throw new InvalidOperationException($"No closed caption part found at {time}.");

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        public override string ToString() => Text;
    }
}