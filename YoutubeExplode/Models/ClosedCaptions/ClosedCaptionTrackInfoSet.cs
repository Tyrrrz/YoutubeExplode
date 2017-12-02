using System.Collections.Generic;
using YoutubeExplode.Internal;

namespace YoutubeExplode.Models.ClosedCaptions
{
    public class ClosedCaptionTrackInfoSet
    {
        public IReadOnlyList<ClosedCaptionTrackInfo> Tracks { get; }

        public ClosedCaptionTrackInfoSet(IReadOnlyList<ClosedCaptionTrackInfo> tracks)
        {
            Tracks = tracks.GuardNotNull(nameof(tracks));
        }
    }
}