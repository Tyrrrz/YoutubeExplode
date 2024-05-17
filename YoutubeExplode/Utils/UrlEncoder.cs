using System.Linq;
using System.Net;

namespace YoutubeExplode.Utils;

/// <summary>
/// Provides methods for encoding URLs.
/// </summary>
public static class UrlEncoder
{
    /// <summary>
    /// Encodes the input string if needed.
    /// </summary>
    /// <param name="input">The input string.</param>
    /// <returns>The encoded string if encoding is needed; otherwise, the original input string.</returns>
    public static string EncodeIfNeeded(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        foreach (char c in input)
        {
            if (!IsUrlSafeChar(c))
            {
                return CustomUrlEncode(input);
            }
        }

        return input;
    }

    private static bool IsUrlSafeChar(char c)
    {
        return (c >= 'A' && c <= 'Z')
            || (c >= 'a' && c <= 'z')
            || (c >= '0' && c <= '9')
            || c == '-'
            || c == '.'
            || c == '_'
            || c == '~'
            || c > 127;
    }

    private static string CustomUrlEncode(string value)
    {
        return string.Join(
            "",
            value.Select(c => IsUrlSafeChar(c) ? c.ToString() : WebUtility.UrlEncode(c.ToString()))
        );
    }
}
