using System.Collections.Generic;

namespace YoutubeExplode.Videos.ClosedCaptions
{
    public class ClosedCaptionTrackManifest
    {
        public IReadOnlyList<ClosedCaptionTrackInfo> Tracks { get; }

        public ClosedCaptionTrackManifest(IReadOnlyList<ClosedCaptionTrackInfo> tracks)
        {
            Tracks = tracks;
        }
    }
}