using System;
using System.Collections.Generic;
using System.Linq;

namespace YoutubeExplode.Videos.ClosedCaptions
{
    /// <summary>
    /// Track that contains closed captions in a specific language.
    /// </summary>
    public class ClosedCaptionTrack
    {
        /// <summary>
        /// Closed captions.
        /// </summary>
        public IReadOnlyList<ClosedCaption> ClosedCaptions { get; }

        /// <summary>
        /// Initializes an instance of <see cref="ClosedCaptionTrack"/>.
        /// </summary>
        public ClosedCaptionTrack(IReadOnlyList<ClosedCaption> closedCaptions)
        {
            ClosedCaptions = closedCaptions;
        }

        /// <summary>
        /// Gets the caption displayed at the specified point in time.
        /// Returns null if not found.
        /// </summary>
        public ClosedCaption? TryGetByTime(TimeSpan time) =>
            ClosedCaptions.FirstOrDefault(c => time >= c.Offset && time <= c.Offset + c.Duration);
    }
}