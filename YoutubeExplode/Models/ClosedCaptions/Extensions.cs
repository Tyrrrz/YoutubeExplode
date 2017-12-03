using System;
using System.Linq;

namespace YoutubeExplode.Models.ClosedCaptions
{
    /// <summary>
    /// Model extensions
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Gets the caption displayed at the given point in time, relative to video's timeline, or null if not found
        /// </summary>
        public static ClosedCaption GetByTime(this ClosedCaptionTrack track, TimeSpan time)
        {
            return track.Captions.FirstOrDefault(c => time >= c.Offset && time <= c.Offset + c.Duration);
        }
    }
}