using System.Collections.Generic;
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
    }
}