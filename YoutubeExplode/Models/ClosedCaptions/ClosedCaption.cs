using System;

namespace YoutubeExplode.Models.ClosedCaptions
{
    /// <summary>
    /// Closed caption
    /// </summary>
    public class ClosedCaption
    {
        /// <summary>
        /// Text displayed by this caption
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// When the caption is shown, relative to video's timeline
        /// </summary>
        public TimeSpan Offset { get; }

        /// <summary>
        /// Duration of this caption
        /// </summary>
        public TimeSpan Duration { get; }

        /// <inheritdoc />
        public ClosedCaption(string text, TimeSpan offset, TimeSpan duration)
        {
            Text = text ?? throw new ArgumentNullException(nameof(text));
            Offset = offset >= TimeSpan.Zero ? offset : throw new ArgumentOutOfRangeException(nameof(offset));
            Duration = duration >= TimeSpan.Zero ? duration : throw new ArgumentOutOfRangeException(nameof(offset));
        }
    }
}