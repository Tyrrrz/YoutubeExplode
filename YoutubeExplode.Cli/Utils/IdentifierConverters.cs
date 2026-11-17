using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using PowerKit.Json.Extensions;
using YoutubeExplode.Channels;
using YoutubeExplode.Playlists;
using YoutubeExplode.Videos;

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

internal class VideoIdConverter() : IdentifierConverter<VideoId>(VideoId.Parse);

internal class PlaylistIdConverter() : IdentifierConverter<PlaylistId>(PlaylistId.Parse);

internal class ChannelIdConverter() : IdentifierConverter<ChannelId>(ChannelId.Parse);

internal class UserNameConverter() : IdentifierConverter<UserName>(UserName.Parse);

internal class ChannelSlugConverter() : IdentifierConverter<ChannelSlug>(ChannelSlug.Parse);

internal class ChannelHandleConverter() : IdentifierConverter<ChannelHandle>(ChannelHandle.Parse);
