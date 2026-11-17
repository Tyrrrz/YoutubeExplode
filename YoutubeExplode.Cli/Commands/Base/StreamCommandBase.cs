using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CliFx.Infrastructure;
using PowerKit.Json.Extensions;
using YoutubeExplode.Common;
using YoutubeExplode.Videos.ClosedCaptions;
using YoutubeExplode.Videos.Streams;

namespace YoutubeExplode.Cli.Commands.Base;

// Commands that operate on a single stream, identified by stream info JSON from stdin
public abstract class StreamCommandBase : CommandBase
{
    private static JsonElement GetRequired(JsonElement element, string name) =>
        element.GetPropertyOrNull(name) is { ValueKind: not JsonValueKind.Null } value
            ? value
            : throw new JsonException($"Property '{name}' is missing.");

    private static Language? TryGetLanguage(JsonElement element, string name) =>
        element.GetPropertyOrNull(name) is { ValueKind: JsonValueKind.Object } language
            ? new Language(
                GetRequired(language, "code").GetNonNullString(),
                GetRequired(language, "name").GetNonNullString()
            )
            : null;

    private static VideoQuality GetVideoQuality(JsonElement element)
    {
        var quality = GetRequired(element, "videoQuality");

        return new VideoQuality(
            GetRequired(quality, "label").GetNonNullString(),
            GetRequired(quality, "maxHeight").GetInt32(),
            GetRequired(quality, "framerate").GetInt32()
        );
    }

    private static Resolution GetResolution(JsonElement element)
    {
        var resolution = GetRequired(element, "videoResolution");

        return new Resolution(
            GetRequired(resolution, "width").GetInt32(),
            GetRequired(resolution, "height").GetInt32()
        );
    }

    protected static async ValueTask<IStreamInfo> ReadStreamInfoAsync(
        IConsole console,
        CancellationToken cancellationToken
    )
    {
        var json = await ReadJsonAsync(console, cancellationToken);

        var url = GetRequired(json, "url").GetNonNullString();
        var container = new Container(
            GetRequired(GetRequired(json, "container"), "name").GetNonNullString()
        );
        var size = new FileSize(GetRequired(GetRequired(json, "size"), "bytes").GetInt64());
        var bitrate = new Bitrate(
            GetRequired(GetRequired(json, "bitrate"), "bitsPerSecond").GetInt64()
        );

        var audioCodec = json.GetPropertyOrNull("audioCodec")?.GetStringOrNull();
        var videoCodec = json.GetPropertyOrNull("videoCodec")?.GetStringOrNull();

        var audioLanguage = TryGetLanguage(json, "audioLanguage");
        var isAudioLanguageDefault = json.GetPropertyOrNull("isAudioLanguageDefault")
            ?.GetBooleanOrNull();
        var isVideoUpscaled =
            json.GetPropertyOrNull("isVideoUpscaled")?.GetBooleanOrNull() ?? false;

        return (audioCodec, videoCodec) switch
        {
            ({ } a, { } v) => new MuxedStreamInfo(
                url,
                container,
                size,
                bitrate,
                a,
                audioLanguage,
                isAudioLanguageDefault,
                v,
                GetVideoQuality(json),
                GetResolution(json),
                isVideoUpscaled
            ),
            ({ } a, null) => new AudioOnlyStreamInfo(
                url,
                container,
                size,
                bitrate,
                a,
                audioLanguage,
                isAudioLanguageDefault
            ),
            (null, { } v) => new VideoOnlyStreamInfo(
                url,
                container,
                size,
                bitrate,
                v,
                GetVideoQuality(json),
                GetResolution(json),
                isVideoUpscaled
            ),
            _ => throw new JsonException(
                "Stream info must contain an audio codec, a video codec, or both."
            ),
        };
    }
}
