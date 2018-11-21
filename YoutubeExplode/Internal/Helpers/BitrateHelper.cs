using System;

namespace YoutubeExplode.Internal.Helpers
{
    internal static class BitrateHelper
    {
        public static long CalculateAverageBitrate(long size, TimeSpan duration)
            => (long) (0.001 * size / (duration.TotalMinutes * 0.0075));
    }
}