using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using YoutubeExplode.Utils.Extensions;

namespace YoutubeExplode.Utils;

internal static class Url
{
    private static IEnumerable<KeyValuePair<string, string>> EnumerateQueryParameters(string url)
    {
        var query = url.Contains('?')
            ? url.SubstringAfter("?")
            : url;

        foreach (var parameter in query.Split("&").Select(WebUtility.UrlDecode))
        {
            // This check is only needed to suppress the NRT warning on UrlDecode
            if (string.IsNullOrWhiteSpace(parameter))
                continue;

            var key = parameter.SubstringUntil("=");
            var value = parameter.SubstringAfter("=");

            if (string.IsNullOrWhiteSpace(key))
                continue;

            yield return new KeyValuePair<string, string>(key, value);
        }
    }

    public static IReadOnlyDictionary<string, string> GetQueryParameters(string url) =>
        EnumerateQueryParameters(url).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

    public static string? TryGetQueryParameter(string url, string key) =>
        EnumerateQueryParameters(url)
            .Where(p => string.Equals(p.Key, key, StringComparison.Ordinal))
            .Select(p => p.Value)
            .FirstOrDefault();

    public static bool ContainsQueryParameter(string url, string key) =>
        TryGetQueryParameter(url, key) is not null;

    public static string SetQueryParameter(string url, string key, string value)
    {
        var parameterEncoded = $"{WebUtility.UrlEncode(key)}={WebUtility.UrlEncode(value)}";

        // Replacing an existing parameter
        var existingMatch = Regex.Match(url, $@"[?&]({Regex.Escape(key)}=?.*?)(?:&|/|$)").Groups[1];
        if (existingMatch.Success)
        {
            return url
                .Remove(existingMatch.Index, existingMatch.Length)
                .Insert(existingMatch.Index, parameterEncoded);
        }
        // Adding a new parameter
        else
        {
            var separator = url.Contains('?') ? '&' : '?';
            return url + separator + parameterEncoded;
        }
    }
}