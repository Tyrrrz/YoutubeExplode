using System.Linq;
using System.Text.Json;

namespace YoutubeExplode.Utils.Extensions
{
    internal static class JsonElementExtensions
    {
        public static JsonElement? GetPropertyOrNull(this JsonElement element, string propertyName)
        {
            if (element.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            if (element.TryGetProperty(propertyName, out var result) &&
                result.ValueKind != JsonValueKind.Null &&
                result.ValueKind != JsonValueKind.Undefined)
            {
                return result;
            }

            return null;
        }

        public static string Flatten(this JsonElement element) => string.Concat(
            element
            .GetPropertyOrNull("runs")?
            .EnumerateArray()
            .Select(x => x.GetPropertyOrNull("text")?.GetString()) ?? Enumerable.Empty<string>()
        );
    }
}