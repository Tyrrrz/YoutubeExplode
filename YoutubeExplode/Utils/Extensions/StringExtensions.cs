using System;
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
    }
}
