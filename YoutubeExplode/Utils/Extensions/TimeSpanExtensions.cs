using System;

namespace YoutubeExplode.Utils.Extensions;

internal static class TimeSpanExtensions
{
    // .NET doesn't have a nice way to render time strings that exceed 24 hours,
    // without including days.
    //
    public static string ToLongString(this TimeSpan value, IFormatProvider? formatProvider = null) =>
        Math.Floor(value.TotalHours).ToString("00", formatProvider) + ':' +
        value.Minutes.ToString("00", formatProvider) + ':' +
        value.Seconds.ToString("00", formatProvider) + '.' +
        value.Milliseconds.ToString("000", formatProvider);
}