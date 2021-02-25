using System.Linq;
using System.Text.Json;

namespace YoutubeExplode.Internal.Extensions
{
    internal static class JsonElementExtensions
    {
        public static JsonElement? GetPropertyOrNull(this JsonElement element, string propertyName) =>
            element.ValueKind != JsonValueKind.Undefined && element.TryGetProperty(propertyName, out var result) ? result : (JsonElement?) null;

        public static string Flatten(this JsonElement element) => string.Concat(
            element
            .GetPropertyOrNull("runs")?
            .EnumerateArray()
            .Select(x => x.GetPropertyOrNull("text")?.GetString()) ?? Enumerable.Empty<string>()
        );
    }
}