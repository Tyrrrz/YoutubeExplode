using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace YoutubeExplode.Utils.Extensions
{
    internal static class StringExtensions
    {
        public static string? NullIfWhiteSpace(this string s) =>
            !string.IsNullOrWhiteSpace(s)
                ? s
                : null;

        public static string SubstringUntil(this string s, string sub,
            StringComparison comparison = StringComparison.Ordinal)
        {
            var index = s.IndexOf(sub, comparison);

            return index < 0
                ? s
                : s.Substring(0, index);
        }

        public static string SubstringAfter(this string s, string sub,
            StringComparison comparison = StringComparison.Ordinal)
        {
            var index = s.IndexOf(sub, comparison);

            return index < 0
                ? string.Empty
                : s.Substring(index + sub.Length, s.Length - index - sub.Length);
        }

        public static string StripNonDigit(this string s)
        {
            var buffer = new StringBuilder();

            foreach (var c in s.Where(char.IsDigit))
            {
                buffer.Append(c);
            }

            return buffer.ToString();
        }

        public static string Reverse(this string s)
        {
            var buffer = new StringBuilder(s.Length);

            for (var i = s.Length - 1; i >= 0; i--)
                buffer.Append(s[i]);

            return buffer.ToString();
        }

        public static string SwapChars(this string s, int firstCharIndex, int secondCharIndex) => new StringBuilder(s)
        {
            [firstCharIndex] = s[secondCharIndex],
            [secondCharIndex] = s[firstCharIndex]
        }.ToString();

        public static int? ParseIntOrNull(this string s) =>
            int.TryParse(s, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out var result)
                ? result
                : null;

        public static int ParseInt(this string s) =>
            ParseIntOrNull(s) ??
            throw new FormatException($"Cannot parse integer number from string '{s}'.");

        public static long? ParseLongOrNull(this string s) =>
            long.TryParse(s, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out var result)
                ? result
                : null;

        public static double? ParseDoubleOrNull(this string s) =>
            double.TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, NumberFormatInfo.InvariantInfo,
                out var result)
                ? result
                : null;

        public static TimeSpan? ParseTimeSpanOrNull(this string s, string[] formats) =>
            TimeSpan.TryParseExact(s, formats, DateTimeFormatInfo.InvariantInfo, out var result)
                ? result
                : null;

        public static string ConcatToString<T>(this IEnumerable<T> source) => string.Concat(source);
    }
}