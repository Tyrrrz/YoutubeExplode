using System;

namespace YoutubeExplode.Converter.Utils.Extensions;

internal static class StringExtensions
{
    extension(string s)
    {
        public string? NullIfWhiteSpace() => !string.IsNullOrWhiteSpace(s) ? s : null;

        public string SubstringUntil(
            string sub,
            StringComparison comparison = StringComparison.Ordinal
        )
        {
            var index = s.IndexOf(sub, comparison);
            return index < 0 ? s : s[..index];
        }
    }
}
