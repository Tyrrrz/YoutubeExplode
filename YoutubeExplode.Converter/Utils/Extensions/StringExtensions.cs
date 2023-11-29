using System;

namespace YoutubeExplode.Converter.Utils.Extensions;

internal static class StringExtensions
{
    public static string? NullIfWhiteSpace(this string s) =>
        !string.IsNullOrWhiteSpace(s) ? s : null;

    public static string SubstringUntil(
        this string str,
        string sub,
        StringComparison comparison = StringComparison.Ordinal
    )
    {
        var index = str.IndexOf(sub, comparison);
        return index < 0 ? str : str[..index];
    }
}
