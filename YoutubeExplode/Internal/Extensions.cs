using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace YoutubeExplode.Internal
{
    internal static class Extensions
    {
        public static bool IsEither<T>(this T value, params T[] potentialValues)
        {
            foreach (var o in potentialValues)
            {
                if (Equals(value, o))
                    return true;
            }
            return false;
        }

        public static bool IsInRange(this IComparable value, IComparable min, IComparable max)
        {
            return value.CompareTo(min) >= 0 && value.CompareTo(max) <= 0;
        }

        public static bool IsBlank(this string str)
        {
            return string.IsNullOrWhiteSpace(str);
        }

        public static bool IsNotBlank(this string str)
        {
            return !string.IsNullOrWhiteSpace(str);
        }

        public static bool EqualsInvariant(this string str, string other)
        {
            str = str?.Trim();
            other = other?.Trim();
            return string.Equals(str, other, StringComparison.OrdinalIgnoreCase);
        }

        public static bool ContainsInvariant(this string str, string other)
        {
            if (str == null)
                throw new ArgumentNullException(nameof(str));
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            return str.IndexOf(other, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public static string SubstringUntil(this string str, string sub, StringComparison comparison = StringComparison.Ordinal)
        {
            if (str == null)
                throw new ArgumentNullException(nameof(str));
            if (sub == null)
                throw new ArgumentNullException(nameof(sub));

            int index = str.IndexOf(sub, comparison);
            if (index < 0) return str;
            return str.Substring(0, index);
        }

        public static string SubstringAfter(this string str, string sub, StringComparison comparison = StringComparison.Ordinal)
        {
            if (str == null)
                throw new ArgumentNullException(nameof(str));
            if (sub == null)
                throw new ArgumentNullException(nameof(sub));

            int index = str.IndexOf(sub, comparison);
            if (index < 0) return string.Empty;
            return str.Substring(index + sub.Length, str.Length - index - sub.Length);
        }

        public static double ParseDouble(this string str)
        {
            if (str == null)
                throw new ArgumentNullException(nameof(str));

            const NumberStyles style = NumberStyles.Float | NumberStyles.AllowThousands;
            return double.Parse(str, style, CultureInfo.InvariantCulture);
        }

        public static int ParseInt(this string str)
        {
            if (str == null)
                throw new ArgumentNullException(nameof(str));

            const NumberStyles style = NumberStyles.Float | NumberStyles.AllowThousands;
            return int.Parse(str, style, CultureInfo.InvariantCulture);
        }

        public static long ParseLong(this string str)
        {
            if (str == null)
                throw new ArgumentNullException(nameof(str));

            const NumberStyles style = NumberStyles.Float | NumberStyles.AllowThousands;
            return long.Parse(str, style, CultureInfo.InvariantCulture);
        }

        public static double ParseDoubleOrDefault(this string str, double defaultValue = default(double))
        {
            if (str == null)
                return defaultValue;

            const NumberStyles style = NumberStyles.Float | NumberStyles.AllowThousands;
            if (double.TryParse(str, style, CultureInfo.InvariantCulture, out double result))
                return result;
            return defaultValue;
        }

        public static int ParseIntOrDefault(this string str, int defaultValue = default(int))
        {
            if (str == null)
                return defaultValue;

            const NumberStyles style = NumberStyles.Float | NumberStyles.AllowThousands;
            if (int.TryParse(str, style, CultureInfo.InvariantCulture, out int result))
                return result;
            return defaultValue;
        }

        public static long ParseLongOrDefault(this string str, long defaultValue = default(long))
        {
            if (str == null)
                return defaultValue;

            const NumberStyles style = NumberStyles.Float | NumberStyles.AllowThousands;
            if (long.TryParse(str, style, CultureInfo.InvariantCulture, out long result))
                return result;
            return defaultValue;
        }

        public static string Reverse(this string str)
        {
            if (str == null)
                throw new ArgumentNullException(nameof(str));

            var sb = new StringBuilder(str.Length);
            for (int i = str.Length - 1; i >= 0; i--)
                sb.Append(str[i]);
            return sb.ToString();
        }

        public static string UrlEncode(this string url)
        {
            if (url == null)
                throw new ArgumentNullException(nameof(url));

            return WebUtility.UrlEncode(url);
        }

        public static string UrlDecode(this string url)
        {
            if (url == null)
                throw new ArgumentNullException(nameof(url));

            return WebUtility.UrlDecode(url);
        }

        public static string SetQueryParameter(this string url, string key, string value)
        {
            if (url == null)
                throw new ArgumentNullException(nameof(url));
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            if (value == null)
                value = string.Empty;

            // Find existing parameter
            var existingMatch = Regex.Match(url, $@"[?&]({Regex.Escape(key)}=?.*?)(?:&|/|$)");

            // Parameter already set to something
            if (existingMatch.Success)
            {
                var group = existingMatch.Groups[1];

                // Remove existing
                url = url.Remove(group.Index, group.Length);

                // Insert new one
                url = url.Insert(group.Index, $"{key}={value}");

                return url;
            }
            // Parameter hasn't been set yet
            else
            {
                // See if there are other parameters
                bool hasOtherParams = url.IndexOf('?') >= 0;

                // Prepend either & or ? depending on that
                char separator = hasOtherParams ? '&' : '?';

                // Assemble new query string
                return url + separator + key + '=' + value;
            }
        }

        public static string SetPathParameter(this string url, string key, string value)
        {
            if (IsBlank(url))
                throw new ArgumentNullException(nameof(url));
            if (IsBlank(key))
                throw new ArgumentNullException(nameof(key));
            if (value == null)
                value = string.Empty;

            // Find existing parameter
            var existingMatch = Regex.Match(url, $@"/({Regex.Escape(key)}/?.*?)(?:/|$)");

            // Parameter already set to something
            if (existingMatch.Success)
            {
                var group = existingMatch.Groups[1];

                // Remove existing
                url = url.Remove(group.Index, group.Length);

                // Insert new one
                url = url.Insert(group.Index, $"{key}/{value}");

                return url;
            }
            // Parameter hasn't been set yet
            else
            {
                // Assemble new query string
                return url + '/' + key + '/' + value;
            }
        }

        public static string JoinToString<T>(this IEnumerable<T> enumerable, string separator)
        {
            if (enumerable == null)
                throw new ArgumentNullException(nameof(enumerable));
            if (separator == null)
                throw new ArgumentNullException(nameof(separator));

            return string.Join(separator, enumerable);
        }

        public static string[] Split(this string input, params string[] separators)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            return input.Split(separators, StringSplitOptions.RemoveEmptyEntries);
        }

        public static IEnumerable<TSource> Distinct<TSource, TKey>(this IEnumerable<TSource> enumerable, Func<TSource, TKey> selector)
        {
            if (enumerable == null)
                throw new ArgumentNullException(nameof(enumerable));
            if (selector == null)
                throw new ArgumentNullException(nameof(selector));

            var existing = new HashSet<TKey>();
            foreach (var element in enumerable)
            {
                if (existing.Add(selector(element)))
                    yield return element;
            }
        }

        public static IEnumerable<T> TakeLast<T>(this IEnumerable<T> enumerable, int count)
        {
            if (enumerable == null)
                throw new ArgumentNullException(nameof(enumerable));
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));
            if (count == 0)
                return Enumerable.Empty<T>();

            return enumerable.Reverse().Take(count).Reverse();
        }

        public static IEnumerable<T> SkipLast<T>(this IEnumerable<T> enumerable, int count)
        {
            if (enumerable == null)
                throw new ArgumentNullException(nameof(enumerable));
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));
            if (count == 0)
                return enumerable;

            return enumerable.Reverse().Skip(count).Reverse();
        }

        public static TValue GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dic, TKey key,
            TValue defaultValue = default(TValue))
        {
            if (dic == null)
                throw new ArgumentNullException(nameof(dic));

            if (dic.TryGetValue(key, out var result))
                return result;
            return defaultValue;
        }

        public static XElement StripNamespaces(this XElement element)
        {
            // Original code credit: http://stackoverflow.com/a/1147012

            if (element == null)
                throw new ArgumentNullException(nameof(element));

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

        public static XElement Descendant(this XElement element, XName name)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            return element.Descendants(name).FirstOrDefault();
        }
    }
}