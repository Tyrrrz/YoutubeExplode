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
        /// Text displayed by this caption.
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// Time at which this caption starts being displayed.
        /// </summary>
        public TimeSpan Offset { get; }

        /// <summary>
        /// Duration this caption is displayed.
        /// </summary>
        public TimeSpan Duration { get; }

        /// <summary>
        /// Caption parts (usually individual words).
        /// May be empty because not all captions contain parts.
        /// </summary>
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
        /// Gets the caption part displayed at the specified point in time, relative to this caption's offset.
        /// Returns null if not found.
        /// Note that some captions may not have any parts at all.
        /// </summary>
        public ClosedCaptionPart? TryGetPartByTime(TimeSpan offset) =>
            Parts.FirstOrDefault(p => p.Offset >= offset);

        /// <inheritdoc />
        public override string ToString() => Text;
    }
}