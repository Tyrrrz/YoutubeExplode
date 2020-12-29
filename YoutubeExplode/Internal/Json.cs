using System.Text;
using System.Text.Json;

namespace YoutubeExplode.Internal
{
    internal static class Json
    {
        public static string Extract(string source)
        {
            StringBuilder sb = new StringBuilder();
            int depth = 0;
            char lastChar = default;
            foreach (var ch in source)
            {
                sb.Append(ch);
                if (ch == '{' && lastChar != '\\')
                    depth++;
                else if (ch == '}' && lastChar != '\\')
                    depth--;
                if (depth == 0)
                    break;
                lastChar = ch;
            }
            return sb.ToString();
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
    }
}
