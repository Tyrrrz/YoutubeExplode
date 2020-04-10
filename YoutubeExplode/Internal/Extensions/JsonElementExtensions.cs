using System.Text.Json;

namespace YoutubeExplode.Internal.Extensions
{
    internal static class JsonElementExtensions
    {
        public static JsonElement? GetPropertyOrNull(this JsonElement element, string propertyName) =>
            element.TryGetProperty(propertyName, out var result) ? result : (JsonElement?) null;

        public static long? GetInt64OrNull(this JsonElement element) =>
            element.TryGetInt64(out var result) ? result : (long?) null;
    }
}