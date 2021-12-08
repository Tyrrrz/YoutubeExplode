using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace YoutubeExplode.Utils
{
    internal static class Json
    {
        public static string Extract(string source)
        {
            var buffer = new StringBuilder();
            var depth = 0;

            var insideString = false;

            // We trust that the source contains valid json, we just need to extract it.
            // To do it, we will be matching curly braces until we even out.
            for (var i = 0; i < source.Length; i++)
            {
                var ch = source[i];
                var chNxt = i < source.Length - 1 ? source[i + 1] : default;

                buffer.Append(ch);

                if (ch == '\\' && insideString && chNxt != default)
                {
                    // we've come across an escaped character.
                    // consume it and the next character.
                    buffer.Append(chNxt);
                    i++; // skip the next character on the loop.
                    continue;
                }

                // an unescaped quote, toggle our inside string status
                if (ch == '"')
                    insideString = !insideString;

                // Match braces that are not inside strings
                if (ch == '{' && !insideString)
                    depth++;
                else if (ch == '}' && !insideString)
                    depth--;

                // Break when evened out
                if (depth == 0)
                    break;
            }

            return buffer.ToString();
        }

        public static JsonElement Parse(string source)
        {
            using var doc = JsonDocument.Parse(source);
            return doc.RootElement.Clone();
        }

        public static JsonElement? TryParse(string source)
        {
            try
            {
                return Parse(source);
            }
            catch (JsonException)
            {
                return null;
            }
        }

        public static HttpContent SerializeToHttpContent(object? obj) => new StringContent(
            JsonSerializer.Serialize(obj),
            Encoding.UTF8,
            "application/json"
        );
    }
}