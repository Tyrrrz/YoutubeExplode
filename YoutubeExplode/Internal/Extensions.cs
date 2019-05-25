using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace YoutubeExplode.Internal
{
    internal static class Extensions
    {
        public static bool IsNullOrWhiteSpace(this string s) => string.IsNullOrWhiteSpace(s);

        public static string EmptyIfNull(this string s) => s ?? string.Empty;

        public static string SubstringUntil(this string s, string sub,
            StringComparison comparison = StringComparison.Ordinal)
        {
            var index = s.IndexOf(sub, comparison);
            return index < 0 ? s : s.Substring(0, index);
        }

        public static string SubstringAfter(this string s, string sub,
            StringComparison comparison = StringComparison.Ordinal)
        {
            var index = s.IndexOf(sub, comparison);
            return index < 0 ? string.Empty : s.Substring(index + sub.Length, s.Length - index - sub.Length);
        }

        public static string StripNonDigit(this string s) => Regex.Replace(s, "\\D", "");

        public static double ParseDouble(this string s)
        {
            const NumberStyles styles = NumberStyles.Float | NumberStyles.AllowThousands;
            var format = NumberFormatInfo.InvariantInfo;

            return double.Parse(s, styles, format);
        }

        public static double ParseDoubleOrDefault(this string s, double defaultValue = default)
        {
            const NumberStyles styles = NumberStyles.Float | NumberStyles.AllowThousands;
            var format = NumberFormatInfo.InvariantInfo;

            return double.TryParse(s, styles, format, out var result)
                ? result
                : defaultValue;
        }

        public static int ParseInt(this string s)
        {
            const NumberStyles styles = NumberStyles.AllowThousands;
            var format = NumberFormatInfo.InvariantInfo;

            return int.Parse(s, styles, format);
        }

        public static int ParseIntOrDefault(this string s, int defaultValue = default)
        {
            const NumberStyles styles = NumberStyles.AllowThousands;
            var format = NumberFormatInfo.InvariantInfo;

            return int.TryParse(s, styles, format, out var result)
                ? result
                : defaultValue;
        }

        public static long ParseLong(this string s)
        {
            const NumberStyles styles = NumberStyles.AllowThousands;
            var format = NumberFormatInfo.InvariantInfo;

            return long.Parse(s, styles, format);
        }

        public static long ParseLongOrDefault(this string s, long defaultValue = default)
        {
            const NumberStyles styles = NumberStyles.AllowThousands;
            var format = NumberFormatInfo.InvariantInfo;

            return long.TryParse(s, styles, format, out var result)
                ? result
                : defaultValue;
        }

        public static DateTimeOffset ParseDateTimeOffset(this string s) =>
            DateTimeOffset.Parse(s, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AssumeUniversal);

        public static DateTimeOffset ParseDateTimeOffset(this string s, string format) =>
            DateTimeOffset.ParseExact(s, format, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AssumeUniversal);

        public static string Reverse(this string s)
        {
            var sb = new StringBuilder(s.Length);

            for (var i = s.Length - 1; i >= 0; i--)
                sb.Append(s[i]);

            return sb.ToString();
        }

        public static string SwapChars(this string s, int firstCharIndex, int secondCharIndex) =>
            new StringBuilder(s)
            {
                [firstCharIndex] = s[secondCharIndex],
                [secondCharIndex] = s[firstCharIndex]
            }.ToString();

        public static string UrlEncode(this string url)
        {
            return WebUtility.UrlEncode(url);
        }

        public static string UrlDecode(this string url)
        {
            return WebUtility.UrlDecode(url);
        }

        public static string[] Split(this string input, params string[] separators) =>
            input.Split(separators, StringSplitOptions.RemoveEmptyEntries);

        public static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T> enumerable) =>
            enumerable ?? Enumerable.Empty<T>();

        public static XElement StripNamespaces(this XElement element)
        {
            // Original code credit: http://stackoverflow.com/a/1147012

            var result = new XElement(element);
            foreach (var e in result.DescendantsAndSelf())
            {
                e.Name = XNamespace.None.GetName(e.Name.LocalName);
                var attributes = e.Attributes()
                    .Where(a => !a.IsNamespaceDeclaration)
                    .Where(a => a.Name.Namespace != XNamespace.Xml && a.Name.Namespace != XNamespace.Xmlns)
                    .Select(a => new XAttribute(XNamespace.None.GetName(a.Name.LocalName), a.Value));
                e.ReplaceAttributes(attributes);
            }

            return result;
        }

        public static TValue GetValueOrDefault<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dic, TKey key) =>
            dic.TryGetValue(key, out var result) ? result : default;

        public static async Task CopyToAsync(this Stream source, Stream destination,
            IProgress<double> progress = null, CancellationToken cancellationToken = default,
            int bufferSize = 81920)
        {
            var buffer = new byte[bufferSize];
            var totalBytesCopied = 0L;
            int bytesCopied;

            do
            {
                // Read
                bytesCopied = await source.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false);

                // Write
                await destination.WriteAsync(buffer, 0, bytesCopied, cancellationToken).ConfigureAwait(false);

                // Report progress
                totalBytesCopied += bytesCopied;
                progress?.Report(1.0 * totalBytesCopied / source.Length);
            } while (bytesCopied > 0);
        }
    }
}