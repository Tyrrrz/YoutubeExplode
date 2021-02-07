using System.Text.Json;

namespace YoutubeExplode.Internal.Extensions
{
    internal static class JsonElementExtensions
    {
        public static JsonElement? GetPropertyOrNull(this JsonElement element, string propertyName) =>
            element.ValueKind != JsonValueKind.Undefined && element.TryGetProperty(propertyName, out var result) ? result : (JsonElement?) null;
    }
}