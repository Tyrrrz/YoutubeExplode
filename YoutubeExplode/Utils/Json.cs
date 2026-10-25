using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using PowerKit.Json.Extensions;

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

    public static string Encode(IReadOnlyDictionary<string, object?> obj)
    {
        using var buffer = new MemoryStream();
        using var writer = new Utf8JsonWriter(buffer);

        writer.WriteValue(obj);
        writer.Flush();

        return Encoding.UTF8.GetString(buffer.ToArray());
    }
}
