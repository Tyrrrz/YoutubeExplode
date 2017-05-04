using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
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

        public static bool IsInRange<T>(this T value, T min, T max) where T : IComparable
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

        public static string SubstringUntil(this string str, string sub, StringComparison comparison = StringComparison.Ordinal)
        {
            int index = str.IndexOf(sub, comparison);
            if (index < 0) return str;
            return str.Substring(0, index);
        }

        public static string SubstringAfter(this string str, string sub, StringComparison comparison = StringComparison.Ordinal)
        {
            int index = str.IndexOf(sub, comparison);
            if (index < 0) return string.Empty;
            return str.Substring(index + sub.Length, str.Length - index - sub.Length);
        }

        public static double ParseDouble(this string str)
        {
            return double.Parse(str, NumberFormatInfo.InvariantInfo);
        }

        public static int ParseInt(this string str)
        {
            return int.Parse(str, NumberFormatInfo.InvariantInfo);
        }

        public static long ParseLong(this string str)
        {
            return long.Parse(str, NumberFormatInfo.InvariantInfo);
        }

        public static string Reverse(this string str)
        {
            var sb = new StringBuilder(str.Length);
            for (int i = str.Length - 1; i >= 0; i--)
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

        public static string JoinToString<T>(this IEnumerable<T> enumerable, string separator)
        {
            return string.Join(separator, enumerable);
        }

        public static string[] Split(this string input, params string[] separators)
        {
            return input.Split(separators, StringSplitOptions.RemoveEmptyEntries);
        }

        public static IEnumerable<TSource> Distinct<TSource, TKey>(this IEnumerable<TSource> enumerable, Func<TSource, TKey> selector)
        {
            var existing = new HashSet<TKey>();
            foreach (var element in enumerable)
            {
                if (existing.Add(selector(element)))
                    yield return element;
            }
        }

        public static TValue Get<TKey, TValue>(this IDictionary<TKey, TValue> dic, TKey key)
        {
            if (dic.TryGetValue(key, out var result))
                return result;
            throw new KeyNotFoundException($"Could not find key [{key}]");
        }

        public static TValue GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dic, TKey key,
            TValue defaultValue = default(TValue))
        {
            if (dic.TryGetValue(key, out var result))
                return result;
            return defaultValue;
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

        public static XElement Descendant(this XElement element, XName name)
        {
            return element.Descendants(name).FirstOrDefault();
        }

        public static XElement ElementStrict(this XElement element, XName name)
        {
            return element.Element(name) ?? throw new KeyNotFoundException($"Could not find element [{name}]");
        }

        public static XAttribute AttributeStrict(this XElement element, XName name)
        {
            return element.Attribute(name) ?? throw new KeyNotFoundException($"Could not find attribute [{name}]");
        }
    }
}