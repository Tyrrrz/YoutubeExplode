using System;
using System.Linq;

namespace YoutubeExplode.Models.ClosedCaptions
{
    /// <summary>
    /// Extensions for <see cref="ClosedCaptions"/>.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Gets <see cref="ClosedCaption"/> displayed at the given point in time.
        /// Returns null if not found.
        /// </summary>
        public static ClosedCaption GetByTime(this ClosedCaptionTrack track, TimeSpan time)
        {
            return track.Captions.FirstOrDefault(c => time >= c.Offset && time <= c.Offset + c.Duration);
        }
    }
}