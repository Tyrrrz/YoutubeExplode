using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using PowerKit.Json.Extensions;

namespace YoutubeExplode.Cli.Utils;

// Identifier structs are represented as plain strings
internal abstract class IdentifierConverter<T>(Func<string, T> parse) : JsonConverter<T>
    where T : struct
{
    public override T Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    ) => parse(JsonElement.ParseValue(ref reader).GetNonWhiteSpaceString());

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.ToString());
}
