using System;

namespace YoutubeExplode.Internal
{
    internal static class Epoch
    {
        public static DateTimeOffset ToDateTimeOffset(long offsetSeconds) =>
            new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero) + TimeSpan.FromSeconds(offsetSeconds);
    }
}