using System;
using System.Collections.Generic;
using System.Linq;

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

        /// <inheritdoc />
        public ClosedCaptionTrack(ClosedCaptionTrackInfo info, IEnumerable<ClosedCaption> captions)
        {
            Info = info ?? throw new ArgumentNullException(nameof(info));
            Captions = captions?.ToArray() ?? throw new ArgumentNullException(nameof(captions));
        }

        /// <summary>
        /// Gets the caption displayed at the given point in time, relative to video's timeline
        /// </summary>
        /// <returns><see cref="ClosedCaption"/> or null if there's no caption shown at given offset</returns>
        public ClosedCaption GetByTime(TimeSpan time)
        {
            return Captions.FirstOrDefault(c => time >= c.Offset && time <= c.Offset + c.Duration);
        }
    }
}