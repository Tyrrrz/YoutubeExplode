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
        public static bool IsBlank(this string str)
        {
            return string.IsNullOrWhiteSpace(str);
        }

        public static bool IsNotBlank(this string str)
        {
            return !string.IsNullOrWhiteSpace(str);
        }

        public static string SubstringUntil(this string str, string sub,
            StringComparison comparison = StringComparison.Ordinal)
        {
            var index = str.IndexOf(sub, comparison);
            return index < 0 ? str : str.Substring(0, index);
        }

        public static string SubstringAfter(this string str, string sub,
            StringComparison comparison = StringComparison.Ordinal)
        {
            var index = str.IndexOf(sub, comparison);
            return index < 0 ? string.Empty : str.Substring(index + sub.Length, str.Length - index - sub.Length);
        }

        public static string StripNonDigit(this string str)
        {
            return Regex.Replace(str, "\\D", "");
        }

        public static double ParseDouble(this string str)
        {
            const NumberStyles styles = NumberStyles.Float | NumberStyles.AllowThousands;
            var format = NumberFormatInfo.InvariantInfo;

            return double.Parse(str, styles, format);
        }

        public static double ParseDoubleOrDefault(this string str, double defaultValue = default(double))
        {
            const NumberStyles styles = NumberStyles.Float | NumberStyles.AllowThousands;
            var format = NumberFormatInfo.InvariantInfo;

            return double.TryParse(str, styles, format, out var result)
                ? result
                : defaultValue;
        }

        public static int ParseInt(this string str)
        {
            const NumberStyles styles = NumberStyles.AllowThousands;
            var format = NumberFormatInfo.InvariantInfo;

            return int.Parse(str, styles, format);
        }

        public static int ParseIntOrDefault(this string str, int defaultValue = default(int))
        {
            const NumberStyles styles = NumberStyles.AllowThousands;
            var format = NumberFormatInfo.InvariantInfo;

            return int.TryParse(str, styles, format, out var result)
                ? result
                : defaultValue;
        }

        public static long ParseLong(this string str)
        {
            const NumberStyles styles = NumberStyles.AllowThousands;
            var format = NumberFormatInfo.InvariantInfo;

            return long.Parse(str, styles, format);
        }

        public static long ParseLongOrDefault(this string str, long defaultValue = default(long))
        {
            const NumberStyles styles = NumberStyles.AllowThousands;
            var format = NumberFormatInfo.InvariantInfo;

            return long.TryParse(str, styles, format, out var result)
                ? result
                : defaultValue;
        }

        public static DateTimeOffset ParseDateTimeOffset(this string str)
        {
            return DateTimeOffset.Parse(str, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AssumeUniversal);
        }

        public static DateTimeOffset ParseDateTimeOffset(this string str, string format)
        {
            return DateTimeOffset.ParseExact(str, format, DateTimeFormatInfo.InvariantInfo,
                DateTimeStyles.AssumeUniversal);
        }

        public static string Reverse(this string str)
        {
            var sb = new StringBuilder(str.Length);

            for (var i = str.Length - 1; i >= 0; i--)
                sb.Append(str[i]);

            return sb.ToString();
        }

        public static string UrlEncode(this string url)
        {
            return WebUtility.UrlEncode(url);
        }

        public static string UrlDecode(this string url)
        {
            return WebUtility.UrlDecode(url);
        }

        public static string HtmlEncode(this string url)
        {
            return WebUtility.HtmlEncode(url);
        }

        public static string HtmlDecode(this string url)
        {
            return WebUtility.HtmlDecode(url);
        }

        public static string JoinToString<T>(this IEnumerable<T> enumerable, string separator)
        {
            return string.Join(separator, enumerable);
        }

        public static string[] Split(this string input, params string[] separators)
        {
            return input.Split(separators, StringSplitOptions.RemoveEmptyEntries);
        }

        public static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T> enumerable)
        {
            return enumerable ?? Enumerable.Empty<T>();
        }

        public static IEnumerable<TSource> Distinct<TSource, TKey>(this IEnumerable<TSource> enumerable,
            Func<TSource, TKey> selector)
        {
            var existing = new HashSet<TKey>();

            foreach (var element in enumerable)
            {
                if (existing.Add(selector(element)))
                    yield return element;
            }
        }

        public static TValue GetOrDefault<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dic, TKey key,
            TValue defaultValue = default(TValue))
        {
            return dic.TryGetValue(key, out var result) ? result : defaultValue;
        }

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

        public static async Task CopyToAsync(this Stream source, Stream destination,
            IProgress<double> progress = null, CancellationToken cancellationToken = default(CancellationToken),
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