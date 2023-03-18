using System;
using System.Text.Json;

namespace YoutubeExplode.Utils;

// Utility to serialize objects to JSON without defining types.
// Required because serializing anonymous types is not possible with assembly trimming.
internal class JsonWriter
{
    private readonly Utf8JsonWriter _writer;

    public JsonWriter(Utf8JsonWriter writer) => _writer = writer;

    public JsonWriter Number(int? value)
    {
        if (value is not null)
            _writer.WriteNumberValue(value.Value);
        else
            _writer.WriteNullValue();

        return this;
    }

    public JsonWriter String(string? value)
    {
        _writer.WriteStringValue(value);
        return this;
    }

    public JsonWriter Object(Action<JsonWriter> write)
    {
        _writer.WriteStartObject();

        var objectWriter = new JsonWriter(_writer);
        write(objectWriter);

        _writer.WriteEndObject();

        return this;
    }

    public JsonWriter Property(string name, Action<JsonWriter> write)
    {
        _writer.WritePropertyName(name);

        var propertyWriter = new JsonWriter(_writer);
        write(propertyWriter);

        return this;
    }
}