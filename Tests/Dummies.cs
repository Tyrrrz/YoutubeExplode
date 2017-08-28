using System;
using YoutubeExplode.Models.ClosedCaptions;

namespace Tests
{
    public static class Dummies
    {
        public static ClosedCaptionTrack GetClosedCaptionTrack()
        {
            var info = new ClosedCaptionTrackInfo("test", new Language("en", "English (auto-generated)"), true);
            return new ClosedCaptionTrack(info,
                new[]
                {
                    new ClosedCaption("Hello", TimeSpan.Zero, TimeSpan.FromSeconds(1)),
                    new ClosedCaption("world", TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(1))
                });
        }
    }
}