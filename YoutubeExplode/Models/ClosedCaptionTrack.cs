using System;
using System.Linq;
using YoutubeExplode.Internal;

namespace YoutubeExplode.Models
{
    /// <summary>
    /// Closed caption track
    /// </summary>
    public class ClosedCaptionTrack
    {
        /// <summary>
        /// Meta data associated with this caption track
        /// </summary>
        public ClosedCaptionTrackInfo Info { get; internal set; }

        /// <summary>
        /// Closed captions inside this track
        /// </summary>
        public ClosedCaption[] Captions { get; internal set; }

        internal ClosedCaptionTrack()
        {
        }

        /// <summary>
        /// Gets caption for given time offset
        /// </summary>
        /// <returns>Found caption or null if there's no caption shown at given offset</returns>
        public ClosedCaption GetByOffset(TimeSpan offset)
        {
            return Captions.FirstOrDefault(c => offset.IsInRange(c.Offset, c.Offset + c.Duration));
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Captions: {Captions.Length}";
        }
    }
}