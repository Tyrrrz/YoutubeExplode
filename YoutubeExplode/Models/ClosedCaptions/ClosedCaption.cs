﻿using System;
using YoutubeExplode.Internal;

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

        /// <summary />
        public ClosedCaption(string text, TimeSpan offset, TimeSpan duration)
        {
            Text = text.GuardNotNull(nameof(text));
            Offset = offset.GuardNotNegative(nameof(offset));
            Duration = duration.GuardNotNegative(nameof(duration));
        }

        /// <inheritdoc />
        public override string ToString() => Text;
    }
}