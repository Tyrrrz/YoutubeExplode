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
        public string Text { get; internal set; }

        /// <summary>
        /// When the caption is shown, relative to video's timeline
        /// </summary>
        public TimeSpan Offset { get; internal set; }

        /// <summary>
        /// Duration of this caption
        /// </summary>
        public TimeSpan Duration { get; internal set; }

        internal ClosedCaption()
        {
        }
    }
}