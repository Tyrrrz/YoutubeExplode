using System.Net.Http;
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
        for (var i = 0; i < source.Length; i++)
        {
            var ch = source[i];
            var chPrv = i > 0 ? source[i - 1] : default;

            buffer.Append(ch);

            // Detect if inside a string
            if (ch == '"' && chPrv != '\\')
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

    public static HttpContent SerializeToHttpContent<T>(T obj) => new StringContent(
        JsonSerializer.Serialize(obj),
        Encoding.UTF8,
        "application/json"
    );
}