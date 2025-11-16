using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace YoutubeExplode.Utils.Extensions;

internal static class StringExtensions
{
    extension(string str)
    {
        public string? NullIfWhiteSpace() => !string.IsNullOrWhiteSpace(str) ? str : null;

        public string SubstringUntil(
            string sub,
            StringComparison comparison = StringComparison.Ordinal
        )
        {
            var index = str.IndexOf(sub, comparison);
            return index < 0 ? str : str[..index];
        }

        public string SubstringAfter(
            string sub,
            StringComparison comparison = StringComparison.Ordinal
        )
        {
            var index = str.IndexOf(sub, comparison);

            return index < 0
                ? string.Empty
                : str.Substring(index + sub.Length, str.Length - index - sub.Length);
        }

        public string StripNonDigit()
        {
            var buffer = new StringBuilder();

            foreach (var c in str.Where(char.IsDigit))
                buffer.Append(c);

            return buffer.ToString();
        }

        public string Reverse()
        {
            var buffer = new StringBuilder(str.Length);

            for (var i = str.Length - 1; i >= 0; i--)
                buffer.Append(str[i]);

            return buffer.ToString();
        }

        public string SwapChars(int firstCharIndex, int secondCharIndex) =>
            new StringBuilder(str)
            {
                [firstCharIndex] = str[secondCharIndex],
                [secondCharIndex] = str[firstCharIndex],
            }.ToString();

        public int? ParseIntOrNull() =>
            int.TryParse(str, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out var result)
                ? result
                : null;

        public int ParseInt() =>
            ParseIntOrNull(str)
            ?? throw new FormatException($"Cannot parse integer number from string '{str}'.");

        public long? ParseLongOrNull() =>
            long.TryParse(str, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out var result)
                ? result
                : null;

        public double? ParseDoubleOrNull() =>
            double.TryParse(
                str,
                NumberStyles.Float | NumberStyles.AllowThousands,
                NumberFormatInfo.InvariantInfo,
                out var result
            )
                ? result
                : null;

        public TimeSpan? ParseTimeSpanOrNull(string[] formats) =>
            TimeSpan.TryParseExact(str, formats, DateTimeFormatInfo.InvariantInfo, out var result)
                ? result
                : null;

        public DateTimeOffset? ParseDateTimeOffsetOrNull() =>
            DateTimeOffset.TryParse(
                str,
                DateTimeFormatInfo.InvariantInfo,
                DateTimeStyles.None,
                out var result
            )
                ? result
                : null;
    }

    extension<T>(IEnumerable<T> source)
    {
        public string ConcatToString() => string.Concat(source);
    }
}
