using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;

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
        foreach (var (i, ch) in source.Index())
        {
            var prev = i > 0 ? source[i - 1] : default;

            buffer.Append(ch);

            // Detect if inside a string
            if (ch == '"' && prev != '\\')
                isInsideString = !isInsideString;
            // Opening brace
            else if (ch == '{' && !isInsideString)
                depth++;
            // Closing brace
            else if (ch == '}' && !isInsideString)
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

    public static string Encode(string value)
    {
        var buffer = new StringBuilder(value.Length);

        foreach (var c in value)
        {
            if (c == '\n')
                buffer.Append("\\n");
            else if (c == '\r')
                buffer.Append("\\r");
            else if (c == '\t')
                buffer.Append("\\t");
            else if (c == '\\')
                buffer.Append("\\\\");
            else if (c == '"')
                buffer.Append("\\\"");
            else
                buffer.Append(c);
        }

        return buffer.ToString();
    }

    // AOT-compatible serialization
    public static string Serialize(string? value) =>
        value is not null ? '"' + Encode(value) + '"' : "null";

    // AOT-compatible serialization
    public static string Serialize(int? value) =>
        value is not null ? value.Value.ToString(CultureInfo.InvariantCulture) : "null";
}
