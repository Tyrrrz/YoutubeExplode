using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace YoutubeExplode.Utils.Extensions;

internal static class JsonExtensions
{
    extension(JsonElement element)
    {
        public JsonElement? GetPropertyOrNull(string propertyName)
        {
            if (element.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            if (
                element.TryGetProperty(propertyName, out var result)
                && result.ValueKind != JsonValueKind.Null
                && result.ValueKind != JsonValueKind.Undefined
            )
            {
                return result;
            }

            return null;
        }

        public bool? GetBooleanOrNull() =>
            element.ValueKind switch
            {
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                _ => null,
            };

        public string? GetStringOrNull() =>
            element.ValueKind == JsonValueKind.String ? element.GetString() : null;

        public int? GetInt32OrNull() =>
            element.ValueKind == JsonValueKind.Number && element.TryGetInt32(out var result)
                ? result
                : null;

        public long? GetInt64OrNull() =>
            element.ValueKind == JsonValueKind.Number && element.TryGetInt64(out var result)
                ? result
                : null;

        public JsonElement.ArrayEnumerator? EnumerateArrayOrNull() =>
            element.ValueKind == JsonValueKind.Array ? element.EnumerateArray() : null;

        public JsonElement.ArrayEnumerator EnumerateArrayOrEmpty() =>
            element.EnumerateArrayOrNull() ?? default;

        public JsonElement.ObjectEnumerator? EnumerateObjectOrNull() =>
            element.ValueKind == JsonValueKind.Object ? element.EnumerateObject() : null;

        public JsonElement.ObjectEnumerator EnumerateObjectOrEmpty() =>
            element.EnumerateObjectOrNull() ?? default;

        public IEnumerable<JsonElement> EnumerateDescendantProperties(string propertyName)
        {
            // Check if this property exists on the current object
            var property = element.GetPropertyOrNull(propertyName);
            if (property is not null)
                yield return property.Value;

            // Recursively check on all array children (if current element is an array)
            var deepArrayDescendants = element
                .EnumerateArrayOrEmpty()
                .SelectMany(j => j.EnumerateDescendantProperties(propertyName));

            foreach (var deepDescendant in deepArrayDescendants)
                yield return deepDescendant;

            // Recursively check on all object children (if current element is an object)
            var deepObjectDescendants = element
                .EnumerateObjectOrEmpty()
                .SelectMany(j => j.Value.EnumerateDescendantProperties(propertyName));

            foreach (var deepDescendant in deepObjectDescendants)
                yield return deepDescendant;
        }
    }
}
