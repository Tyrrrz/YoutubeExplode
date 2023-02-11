using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace YoutubeExplode.Utils.Extensions;

internal static class JsonExtensions
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

    public static string? GetStringOrNull(this JsonElement element) =>
        element.ValueKind == JsonValueKind.String
            ? element.GetString()
            : null;

    public static int? GetInt32OrNull(this JsonElement element) =>
        element.ValueKind == JsonValueKind.Number && element.TryGetInt32(out var result)
            ? result
            : null;

    public static long? GetInt64OrNull(this JsonElement element) =>
        element.ValueKind == JsonValueKind.Number && element.TryGetInt64(out var result)
            ? result
            : null;

    public static JsonElement.ArrayEnumerator? EnumerateArrayOrNull(this JsonElement element) =>
        element.ValueKind == JsonValueKind.Array
            ? element.EnumerateArray()
            : null;

    public static JsonElement.ArrayEnumerator EnumerateArrayOrEmpty(this JsonElement element) =>
        element.EnumerateArrayOrNull() ?? default;

    public static JsonElement.ObjectEnumerator? EnumerateObjectOrNull(this JsonElement element) =>
        element.ValueKind == JsonValueKind.Object
            ? element.EnumerateObject()
            : null;

    public static JsonElement.ObjectEnumerator EnumerateObjectOrEmpty(this JsonElement element) =>
        element.EnumerateObjectOrNull() ?? default;

    public static IEnumerable<JsonElement> EnumerateDescendantProperties(
        this JsonElement element,
        string propertyName)
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