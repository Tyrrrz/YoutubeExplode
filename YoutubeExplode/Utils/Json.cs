using System.Text;
using System.Text.Json;
using YoutubeExplode.Utils.Extensions;

namespace YoutubeExplode.Utils;

internal static class Json
{
    public static string Extract(string source)
    {
        var buffer = new StringBuilder();

        var depth = 0;
        var isInsideString = false;

        // We trust that the source contains valid json, we just need to extract it.
        // To do it, we will be matching curly braces until we even out.
        foreach (var (c, i) in source.WithIndex())
        {
            var prev = i > 0 ? source[i - 1] : default;

            buffer.Append(c);

            // Detect if inside a string
            if (c == '"' && prev != '\\')
                isInsideString = !isInsideString;
            // Opening brace
            else if (c == '{' && !isInsideString)
                depth++;
            // Closing brace
            else if (c == '}' && !isInsideString)
                depth--;

            // Break when evened out
            if (depth == 0)
                break;
        }

        return buffer.ToString();
    }

    public static JsonElement Parse(string source)
    {
        using var document = JsonDocument.Parse(source);
        return document.RootElement.Clone();
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