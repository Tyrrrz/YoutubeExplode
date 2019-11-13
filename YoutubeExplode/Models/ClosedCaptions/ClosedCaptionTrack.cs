using System.Collections.Generic;

namespace YoutubeExplode.Models.ClosedCaptions
{
    /// <summary>
    /// Set of captions that get displayed during video playback.
    /// </summary>
    public class ClosedCaptionTrack
    {
        /// <summary>
        /// Metadata associated with this track.
        /// </summary>
        public ClosedCaptionTrackInfo Info { get; }

        /// <summary>
        /// Collection of closed captions that belong to this track.
        /// </summary>
        public IReadOnlyList<ClosedCaption> Captions { get; }

        /// <summary>
        /// Initializes an instance of <see cref="ClosedCaptionTrack"/>.
        /// </summary>
        public ClosedCaptionTrack(ClosedCaptionTrackInfo info, IReadOnlyList<ClosedCaption> captions)
        {
            Info = info;
            Captions = captions;
        }
    }
}