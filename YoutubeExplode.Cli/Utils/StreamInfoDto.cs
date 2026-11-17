using System.Text.Json;
using YoutubeExplode.Common;
using YoutubeExplode.Videos.ClosedCaptions;
using YoutubeExplode.Videos.Streams;

namespace YoutubeExplode.Cli.Utils;

// Stream info types have multiple public constructors, so they can't be deserialized directly.
// This type mirrors the union of their properties and is converted to the concrete type afterwards.
internal record StreamInfoDto(
    string Url,
    Container Container,
    FileSize Size,
    Bitrate Bitrate,
    string? AudioCodec,
    Language? AudioLanguage,
    bool? IsAudioLanguageDefault,
    string? VideoCodec,
    VideoQuality? VideoQuality,
    Resolution? VideoResolution,
    bool IsVideoUpscaled
)
{
    public IStreamInfo ToStreamInfo() =>
        (AudioCodec, VideoCodec, VideoQuality, VideoResolution) switch
        {
            ({ } a, { } v, { } q, { } r) => new MuxedStreamInfo(
                Url,
                Container,
                Size,
                Bitrate,
                a,
                AudioLanguage,
                IsAudioLanguageDefault,
                v,
                q,
                r,
                IsVideoUpscaled
            ),
            ({ } a, null, _, _) => new AudioOnlyStreamInfo(
                Url,
                Container,
                Size,
                Bitrate,
                a,
                AudioLanguage,
                IsAudioLanguageDefault
            ),
            (null, { } v, { } q, { } r) => new VideoOnlyStreamInfo(
                Url,
                Container,
                Size,
                Bitrate,
                v,
                q,
                r,
                IsVideoUpscaled
            ),
            _ => throw new JsonException(
                "Stream info must contain an audio codec, a video codec with its quality and resolution, or both."
            ),
        };
}
