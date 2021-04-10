using System;
using System.Collections.Generic;
using System.Linq;

namespace YoutubeExplode.Videos.ClosedCaptions
{
    /// <summary>
    /// Text that gets displayed at specific time during video playback, as part of a <see cref="ClosedCaptionTrack"/>.
    /// </summary>
    public class ClosedCaption
    {
        /// <summary>
        /// Text displayed by the caption.
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// Time at which the caption is displayed.
        /// </summary>
        public TimeSpan Offset { get; }

        /// <summary>
        /// Duration of the caption.
        /// </summary>
        public TimeSpan Duration { get; }

        /// <summary>
        /// Caption parts (usually individual words).
        /// </summary>
        /// <remarks>
        /// Some captions may not have parts.
        /// </remarks>
        public IReadOnlyList<ClosedCaptionPart> Parts { get; }

        /// <summary>
        /// Initializes an instance of <see cref="ClosedCaption"/>.
        /// </summary>
        public ClosedCaption(string text, TimeSpan offset, TimeSpan duration, IReadOnlyList<ClosedCaptionPart> parts)
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
        /// <remarks>
        /// Some captions may not have parts.
        /// </remarks>
        public ClosedCaptionPart? TryGetPartByTime(TimeSpan time) =>
            Parts.FirstOrDefault(p => p.Offset >= time);

        /// <summary>
        /// Gets the caption part displayed at the specified point in time, relative to the caption's own offset.
        /// </summary>
        /// <remarks>
        /// Some captions may not have parts.
        /// </remarks>
        public ClosedCaptionPart GetPartByTime(TimeSpan time) =>
            TryGetPartByTime(time) ??
            throw new InvalidOperationException($"No closed caption part found at {time}.");

        /// <inheritdoc />
        public override string ToString() => Text;
    }
}