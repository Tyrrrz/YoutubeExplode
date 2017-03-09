using System;

namespace YoutubeExplode.Models
{
    /// <summary>
    /// Closed caption
    /// </summary>
    public class ClosedCaption
    {
        /// <summary>
        /// Text shown by this caption
        /// </summary>
        public string Text { get; internal set; }

        /// <summary>
        /// Offset from the start of the video, when the caption is shown
        /// </summary>
        public TimeSpan Offset { get; internal set; }

        /// <summary>
        /// Duration of time the caption is shown
        /// </summary>
        public TimeSpan Duration { get; internal set; }

        internal ClosedCaption()
        {
        }
    }
}