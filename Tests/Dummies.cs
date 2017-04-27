using System;
using System.Globalization;
using YoutubeExplode.Models.ClosedCaptions;

namespace YoutubeExplode.Tests
{
    public static class Dummies
    {
        public static ClosedCaptionTrack GetClosedCaptionTrack()
        {
            var info = new ClosedCaptionTrackInfo("test", new CultureInfo("en"), true);
            return new ClosedCaptionTrack(info,
                new[]
                {
                    new ClosedCaption("Hello", TimeSpan.Zero, TimeSpan.FromSeconds(1)),
                    new ClosedCaption("world", TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(1))
                });
        }
    }
}