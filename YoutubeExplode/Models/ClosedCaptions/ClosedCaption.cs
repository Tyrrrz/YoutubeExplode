using System;
using YoutubeExplode.Internal;

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
        /// When this caption is displayed, relative to video's timeline
        /// </summary>
        public TimeSpan Offset { get; }

        /// <summary>
        /// How long this caption is displayed
        /// </summary>
        public TimeSpan Duration { get; }

        /// <summary />
        public ClosedCaption(string text, TimeSpan offset, TimeSpan duration)
        {
            Text = text.GuardNotNull(nameof(text));
            Offset = offset.GuardNotNegative(nameof(offset));
            Duration = duration.GuardNotNegative(nameof(duration));
        }
    }
}