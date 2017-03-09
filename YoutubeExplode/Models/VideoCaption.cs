using System;

namespace YoutubeExplode.Models
{
    /// <summary>
    /// Video caption
    /// </summary>
    public class VideoCaption
    {
        /// <summary>
        /// Actual text
        /// </summary>
        public string Text { get; internal set; }

        /// <summary>
        /// Offset from the start of the video, when the caption is shown
        /// </summary>
        public TimeSpan Offset { get; internal set; }

        /// <summary>
        /// Duration of the caption
        /// </summary>
        public TimeSpan Duration { get; internal set; }

        internal VideoCaption()
        {
        }
    }
}