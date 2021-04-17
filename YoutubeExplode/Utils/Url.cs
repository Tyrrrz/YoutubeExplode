using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using YoutubeExplode.Utils.Extensions;

namespace YoutubeExplode.Utils
{
    internal static class Url
    {
        public static string SetQueryParameter(string url, string key, string value)
        {
            var existingMatch = Regex.Match(url, $"[?&]({Regex.Escape(key)}=?.*?)(?:&|/|$)");

            // Parameter has already been set to something
            if (existingMatch.Success)
            {
                var group = existingMatch.Groups[1];

                return url
                    .Remove(group.Index, group.Length)
                    .Insert(group.Index, $"{key}={value}");
            }
            // Parameter hasn't been set yet
            else
            {
                var hasOtherParams = url.IndexOf('?') >= 0;
                var separator = hasOtherParams ? '&' : '?';

                return url + separator + key + '=' + value;
            }
        }

        public static string SetRouteParameter(string url, string key, string value)
        {
            var existingMatch = Regex.Match(url, $"/({Regex.Escape(key)}/?.*?)(?:/|$)");

            // Parameter has already been set to something
            if (existingMatch.Success)
            {
                var group = existingMatch.Groups[1];

                return url
                    .Remove(group.Index, group.Length)
                    .Insert(group.Index, $"{key}/{value}");
            }
            // Parameter hasn't been set yet
            else
            {
                return $"{url}/{key}/{value}";
            }
        }

        public static IReadOnlyDictionary<string, string> SplitQuery(string query)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            var paramsEncoded = query.Split("&");

            foreach (var paramEncoded in paramsEncoded)
            {
                var param = WebUtility.UrlDecode(paramEncoded);

                var key = param.SubstringUntil("=");
                var value = param.SubstringAfter("=");

                if (string.IsNullOrWhiteSpace(key))
                    continue;

                result[key] = value;
            }

            return result;
        }
    }
}