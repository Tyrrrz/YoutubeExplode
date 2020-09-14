using System.Text.Json;

namespace YoutubeExplode.Internal
{
    internal static class Json
    {
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