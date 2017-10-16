using System;
using System.Collections.Generic;
using System.Linq;
using YoutubeExplode.Internal;

namespace YoutubeExplode.Models.ClosedCaptions
{
    /// <summary>
    /// Closed caption track
    /// </summary>
    public class ClosedCaptionTrack
    {
        /// <summary>
        /// Metadata associated with this caption track
        /// </summary>
        public ClosedCaptionTrackInfo Info { get; }

        /// <summary>
        /// Closed captions contained inside this track
        /// </summary>
        public IReadOnlyList<ClosedCaption> Captions { get; }

        /// <summary />
        public ClosedCaptionTrack(ClosedCaptionTrackInfo info, IReadOnlyList<ClosedCaption> captions)
        {
            Info = info.GuardNotNull(nameof(info));
            Captions = captions.GuardNotNull(nameof(captions));
        }

        /// <summary>
        /// Gets the caption displayed at the given point in time, relative to video's timeline, or null if not found
        /// </summary>
        public ClosedCaption GetByTime(TimeSpan time)
        {
            return Captions.FirstOrDefault(c => time >= c.Offset && time <= c.Offset + c.Duration);
        }
    }
}