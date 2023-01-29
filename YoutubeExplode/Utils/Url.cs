using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using YoutubeExplode.Utils.Extensions;

namespace YoutubeExplode.Utils;

internal static class Url
{
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

    public static IReadOnlyDictionary<string, string> GetQueryParameters(string url)
    {
        var query = url.Contains('?')
            ? url.SubstringAfter("?")
            : url;

        var parameters = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var parameterEncoded in query.Split("&"))
        {
            var parameterRaw = WebUtility.UrlDecode(parameterEncoded);

            var key = parameterRaw.SubstringUntil("=");
            var value = parameterRaw.SubstringAfter("=");

            if (string.IsNullOrWhiteSpace(key))
                continue;

            parameters[key] = value;
        }

        return parameters;
    }
}