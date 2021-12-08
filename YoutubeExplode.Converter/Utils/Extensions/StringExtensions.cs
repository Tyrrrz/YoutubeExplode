namespace YoutubeExplode.Converter.Utils.Extensions;

internal static class StringExtensions
{
    public static string? NullIfWhiteSpace(this string s) =>
        !string.IsNullOrWhiteSpace(s)
            ? s
            : null;
}