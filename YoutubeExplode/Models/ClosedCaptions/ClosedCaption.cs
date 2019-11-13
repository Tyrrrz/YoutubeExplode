using System;

namespace YoutubeExplode.Models.ClosedCaptions
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
        /// Initializes an instance of <see cref="ClosedCaption"/>.
        /// </summary>
        public ClosedCaption(string text, TimeSpan offset, TimeSpan duration)
        {
            Text = text;
            Offset = offset;
            Duration = duration;
        }

        /// <inheritdoc />
        public override string ToString() => Text;
    }
}