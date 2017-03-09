using System;
using System.Linq;
using YoutubeExplode.Internal;

namespace YoutubeExplode.Models
{
    /// <summary>
    /// Video caption track
    /// </summary>
    public class VideoCaptionTrack
    {
        /// <summary>
        /// Captions inside this track
        /// </summary>
        public VideoCaption[] Captions { get; internal set; }

        internal VideoCaptionTrack()
        {
        }

        /// <summary>
        /// Gets caption for given time offset
        /// </summary>
        /// <returns>Found caption or null if there's no caption for given offset</returns>
        public VideoCaption GetCaptionFor(TimeSpan offset)
        {
            return Captions.FirstOrDefault(c => offset.IsInRange(c.Offset, c.Offset + c.Duration));
        }
    }
}