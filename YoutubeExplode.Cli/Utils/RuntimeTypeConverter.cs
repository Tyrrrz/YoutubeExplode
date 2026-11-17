using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace YoutubeExplode.Cli.Utils;

// Interfaces are written using their runtime type, so that all type-specific properties are included
internal abstract class RuntimeTypeConverter<T> : JsonConverter<T>
    where T : class
{
    public override T Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    ) => throw new NotSupportedException();

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options) =>
        JsonSerializer.Serialize(writer, value, options.GetTypeInfo(value.GetType()));
}
