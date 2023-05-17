using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace YoutubeExplode.Utils.Extensions;

internal static class StringExtensions
{
    public static string? NullIfWhiteSpace(this string str) =>
        !string.IsNullOrWhiteSpace(str)
            ? str
            : null;

    public static string SubstringUntil(
        this string str,
        string sub,
        StringComparison comparison = StringComparison.Ordinal)
    {
        var index = str.IndexOf(sub, comparison);

        return index < 0
            ? str
            : str[..index];
    }

    public static string SubstringAfter(
        this string str,
        string sub,
        StringComparison comparison = StringComparison.Ordinal)
    {
        var index = str.IndexOf(sub, comparison);

        return index < 0
            ? string.Empty
            : str.Substring(index + sub.Length, str.Length - index - sub.Length);
    }

    public static string StripNonDigit(this string str)
    {
        var buffer = new StringBuilder();

        foreach (var c in str.Where(char.IsDigit))
            buffer.Append(c);

        return buffer.ToString();
    }

    public static string Reverse(this string str)
    {
        var buffer = new StringBuilder(str.Length);

        for (var i = str.Length - 1; i >= 0; i--)
            buffer.Append(str[i]);

        return buffer.ToString();
    }

    public static string SwapChars(this string str, int firstCharIndex, int secondCharIndex) =>
        new StringBuilder(str)
        {
            [firstCharIndex] = str[secondCharIndex],
            [secondCharIndex] = str[firstCharIndex]
        }.ToString();

    public static int? ParseIntOrNull(this string str) =>
        int.TryParse(str, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out var result)
            ? result
            : null;

    public static int ParseInt(this string str) =>
        ParseIntOrNull(str) ??
        throw new FormatException($"Cannot parse integer number from string '{str}'.");

    public static long? ParseLongOrNull(this string str) =>
        long.TryParse(str, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out var result)
            ? result
            : null;

    public static double? ParseDoubleOrNull(this string str) =>
        double.TryParse(str, NumberStyles.Float | NumberStyles.AllowThousands, NumberFormatInfo.InvariantInfo,
            out var result)
            ? result
            : null;

    public static TimeSpan? ParseTimeSpanOrNull(this string str, string[] formats) =>
        TimeSpan.TryParseExact(str, formats, DateTimeFormatInfo.InvariantInfo, out var result)
            ? result
            : null;

    public static DateTimeOffset? ParseDateTimeOffsetOrNull(this string str, string[] formats) =>
        DateTimeOffset.TryParseExact(
            str,
            formats,
            DateTimeFormatInfo.InvariantInfo,
            DateTimeStyles.None,
            out var result)
            ? result
            : null;

    public static string ConcatToString<T>(this IEnumerable<T> source) => string.Concat(source);
}