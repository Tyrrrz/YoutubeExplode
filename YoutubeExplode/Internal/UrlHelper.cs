using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace YoutubeExplode.Internal
{
    internal static class UrlHelper
    {
        public static string SetUrlQueryParameter(string url, string key, string value)
        {
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

        public static string SetUrlPathParameter(string url, string key, string value)
        {
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

        public static IDictionary<string, string> DictionaryFromUrlEncoded(string urlEncoded)
        {
            var dic = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var keyValuePairsRaw = urlEncoded.Split("&");
            foreach (string keyValuePairRaw in keyValuePairsRaw)
            {
                string keyValuePairRawDecoded = keyValuePairRaw.UrlDecode();

                // Look for the equals sign
                int equalsPos = keyValuePairRawDecoded.IndexOf('=');
                if (equalsPos <= 0)
                    continue;

                // Get the key and value
                string key = keyValuePairRawDecoded.Substring(0, equalsPos);
                string value = equalsPos < keyValuePairRawDecoded.Length
                    ? keyValuePairRawDecoded.Substring(equalsPos + 1)
                    : string.Empty;

                // Add to dictionary
                dic[key] = value;
            }

            return dic;
        }
    }
}