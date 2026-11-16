using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using YoutubeExplode.Channels;
using YoutubeExplode.Common;
using YoutubeExplode.Playlists;
using YoutubeExplode.Search;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.ClosedCaptions;
using YoutubeExplode.Videos.Streams;

namespace YoutubeExplode.Cli.Utils;

[JsonSerializable(typeof(Video))]
[JsonSerializable(typeof(Playlist))]
[JsonSerializable(typeof(Channel))]
[JsonSerializable(typeof(ClosedCaptionTrack))]
[JsonSerializable(typeof(ClosedCaptionTrackInfo))]
[JsonSerializable(typeof(IStreamInfo))]
[JsonSerializable(typeof(ISearchResult))]
[JsonSerializable(typeof(AudioOnlyStreamInfo))]
[JsonSerializable(typeof(VideoOnlyStreamInfo))]
[JsonSerializable(typeof(MuxedStreamInfo))]
[JsonSerializable(typeof(VideoSearchResult))]
[JsonSerializable(typeof(PlaylistSearchResult))]
[JsonSerializable(typeof(ChannelSearchResult))]
[JsonSerializable(typeof(IReadOnlyList<IStreamInfo>))]
[JsonSerializable(typeof(IReadOnlyList<ClosedCaptionTrackInfo>))]
[JsonSerializable(typeof(IReadOnlyList<ISearchResult>))]
[JsonSerializable(typeof(IReadOnlyList<VideoSearchResult>))]
[JsonSerializable(typeof(IReadOnlyList<PlaylistSearchResult>))]
[JsonSerializable(typeof(IReadOnlyList<ChannelSearchResult>))]
[JsonSerializable(typeof(IReadOnlyList<PlaylistVideo>))]
internal partial class JsonContext : JsonSerializerContext;

internal static class Json
{
    public static JsonSerializerContext CreateContext(bool indented) =>
        new JsonContext(
            new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = indented,
                Converters =
                {
                    new IdentifierConverter(),
                    new ContainerConverter(),
                    new StreamInfoConverter(),
                    new SearchResultConverter(),
                    new ClosedCaptionTrackInfoConverter(),
                },
            }
        );

    // Identifier structs are serialized as plain strings
    private class IdentifierConverter : JsonConverter<object>
    {
        public override bool CanConvert(Type typeToConvert) =>
            typeToConvert == typeof(VideoId)
            || typeToConvert == typeof(PlaylistId)
            || typeToConvert == typeof(ChannelId)
            || typeToConvert == typeof(UserName)
            || typeToConvert == typeof(ChannelSlug)
            || typeToConvert == typeof(ChannelHandle);

        public override object Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options
        ) => throw new NotSupportedException();

        public override void Write(
            Utf8JsonWriter writer,
            object value,
            JsonSerializerOptions options
        ) => writer.WriteStringValue(value.ToString());
    }

    private class ContainerConverter : JsonConverter<Container>
    {
        public override Container Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options
        ) => new(reader.GetString() ?? throw new JsonException("Container name is missing."));

        public override void Write(
            Utf8JsonWriter writer,
            Container value,
            JsonSerializerOptions options
        ) => writer.WriteStringValue(value.Name);
    }

    private class StreamInfoConverter : JsonConverter<IStreamInfo>
    {
        private static Language? ReadLanguage(JsonElement element, string name) =>
            element.TryGetProperty(name, out var e) && e.ValueKind == JsonValueKind.Object
                ? new Language(
                    e.GetProperty("code").GetString()!,
                    e.GetProperty("name").GetString()!
                )
                : null;

        private static VideoQuality ReadVideoQuality(JsonElement element)
        {
            var e = element.GetProperty("videoQuality");
            return new VideoQuality(
                e.GetProperty("label").GetString()!,
                e.GetProperty("maxHeight").GetInt32(),
                e.GetProperty("framerate").GetInt32()
            );
        }

        private static Resolution ReadResolution(JsonElement element)
        {
            var e = element.GetProperty("videoResolution");
            return new Resolution(
                e.GetProperty("width").GetInt32(),
                e.GetProperty("height").GetInt32()
            );
        }

        private static bool? ReadBoolean(JsonElement element, string name) =>
            element.TryGetProperty(name, out var e)
            && e.ValueKind is JsonValueKind.True or JsonValueKind.False
                ? e.GetBoolean()
                : null;

        public override IStreamInfo Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options
        )
        {
            var e = JsonElement.ParseValue(ref reader);

            var url = e.GetProperty("url").GetString()!;
            var container = new Container(e.GetProperty("container").GetString()!);
            var size = new FileSize(e.GetProperty("size").GetProperty("bytes").GetInt64());
            var bitrate = new Bitrate(
                e.GetProperty("bitrate").GetProperty("bitsPerSecond").GetInt64()
            );

            var hasAudio = e.TryGetProperty("audioCodec", out var audioCodec);
            var hasVideo = e.TryGetProperty("videoCodec", out var videoCodec);

            if (hasAudio && hasVideo)
            {
                return new MuxedStreamInfo(
                    url,
                    container,
                    size,
                    bitrate,
                    audioCodec.GetString()!,
                    ReadLanguage(e, "audioLanguage"),
                    ReadBoolean(e, "isAudioLanguageDefault"),
                    videoCodec.GetString()!,
                    ReadVideoQuality(e),
                    ReadResolution(e),
                    ReadBoolean(e, "isVideoUpscaled") ?? false
                );
            }

            if (hasAudio)
            {
                return new AudioOnlyStreamInfo(
                    url,
                    container,
                    size,
                    bitrate,
                    audioCodec.GetString()!,
                    ReadLanguage(e, "audioLanguage"),
                    ReadBoolean(e, "isAudioLanguageDefault")
                );
            }

            if (hasVideo)
            {
                return new VideoOnlyStreamInfo(
                    url,
                    container,
                    size,
                    bitrate,
                    videoCodec.GetString()!,
                    ReadVideoQuality(e),
                    ReadResolution(e),
                    ReadBoolean(e, "isVideoUpscaled") ?? false
                );
            }

            throw new JsonException(
                "Stream info must contain an audio codec, a video codec, or both."
            );
        }

        // Serialize using the runtime type, so that all stream-specific properties are included
        public override void Write(
            Utf8JsonWriter writer,
            IStreamInfo value,
            JsonSerializerOptions options
        ) => JsonSerializer.Serialize(writer, value, value.GetType(), options);
    }

    private class ClosedCaptionTrackInfoConverter : JsonConverter<ClosedCaptionTrackInfo>
    {
        public override ClosedCaptionTrackInfo Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options
        )
        {
            var e = JsonElement.ParseValue(ref reader);

            var language =
                e.TryGetProperty("language", out var l) && l.ValueKind == JsonValueKind.Object
                    ? new Language(
                        l.GetProperty("code").GetString()!,
                        l.GetProperty("name").GetString()!
                    )
                    : default;

            var isAutoGenerated =
                e.TryGetProperty("isAutoGenerated", out var a) && a.ValueKind == JsonValueKind.True;

            return new ClosedCaptionTrackInfo(
                e.GetProperty("url").GetString()!,
                language,
                isAutoGenerated
            );
        }

        public override void Write(
            Utf8JsonWriter writer,
            ClosedCaptionTrackInfo value,
            JsonSerializerOptions options
        )
        {
            writer.WriteStartObject();
            writer.WriteString("url", value.Url);
            writer.WritePropertyName("language");
            JsonSerializer.Serialize(writer, value.Language, options);
            writer.WriteBoolean("isAutoGenerated", value.IsAutoGenerated);
            writer.WriteEndObject();
        }
    }

    // Serialize using the runtime type, so that all result-specific properties are included
    private class SearchResultConverter : JsonConverter<ISearchResult>
    {
        public override ISearchResult Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options
        ) => throw new NotSupportedException();

        public override void Write(
            Utf8JsonWriter writer,
            ISearchResult value,
            JsonSerializerOptions options
        ) => JsonSerializer.Serialize(writer, value, value.GetType(), options);
    }
}
