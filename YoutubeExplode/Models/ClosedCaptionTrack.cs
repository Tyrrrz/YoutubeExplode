using System;
using System.Collections.Generic;
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
        /// Metadata associated with this caption track
        /// </summary>
        public ClosedCaptionTrackInfo Info { get; internal set; }

        /// <summary>
        /// Closed captions contained inside this track
        /// </summary>
        public IReadOnlyList<ClosedCaption> Captions { get; internal set; }

        internal ClosedCaptionTrack()
        {
        }

        /// <summary>
        /// Gets the caption displayed at the given point in time, relative to video's timeline
        /// </summary>
        /// <returns><see cref="ClosedCaption"/> or null if there's no caption shown at given offset</returns>
        public ClosedCaption GetByTime(TimeSpan time)
        {
            return Captions.FirstOrDefault(c => time.IsInRange(c.Offset, c.Offset + c.Duration));
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Captions.Count} captions";
        }
    }
}