using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Lazy;
using YoutubeExplode.Utils;
using YoutubeExplode.Utils.Extensions;

namespace YoutubeExplode.Bridge;

internal partial class PlayerResponse
{
    private readonly JsonElement _content;

    [Lazy]
    private JsonElement? Playability => _content.GetPropertyOrNull("playabilityStatus");

    [Lazy]
    private string? PlayabilityStatus => Playability?.GetPropertyOrNull("status")?.GetStringOrNull();

    [Lazy]
    public string? PlayabilityError => Playability?.GetPropertyOrNull("reason")?.GetStringOrNull();

    [Lazy]
    public bool IsAvailable =>
        !string.Equals(PlayabilityStatus, "error", StringComparison.OrdinalIgnoreCase) &&
        Details is not null;

    [Lazy]
    public bool IsPlayable => string.Equals(PlayabilityStatus, "ok", StringComparison.OrdinalIgnoreCase);

    [Lazy]
    private JsonElement? Details => _content.GetPropertyOrNull("videoDetails");

    [Lazy]
    public string? Title => Details?.GetPropertyOrNull("title")?.GetStringOrNull();

    [Lazy]
    public string? ChannelId => Details?.GetPropertyOrNull("channelId")?.GetStringOrNull();

    [Lazy]
    public string? Author => Details?.GetPropertyOrNull("author")?.GetStringOrNull();

    [Lazy]
    public DateTimeOffset? UploadDate => _content
        .GetPropertyOrNull("microformat")?
        .GetPropertyOrNull("playerMicroformatRenderer")?
        .GetPropertyOrNull("uploadDate")?
        .GetDateTimeOffset();

    [Lazy]
    public TimeSpan? Duration => Details?
        .GetPropertyOrNull("lengthSeconds")?
        .GetStringOrNull()?
        .ParseDoubleOrNull()?
        .Pipe(TimeSpan.FromSeconds);

    [Lazy]
    public IReadOnlyList<ThumbnailData> Thumbnails => Details?
        .GetPropertyOrNull("thumbnail")?
        .GetPropertyOrNull("thumbnails")?
        .EnumerateArrayOrNull()?
        .Select(j => new ThumbnailData(j))
        .ToArray() ?? Array.Empty<ThumbnailData>();

    public IReadOnlyList<string> Keywords => Details?
        .GetPropertyOrNull("keywords")?
        .EnumerateArrayOrNull()?
        .Select(j => j.GetStringOrNull())
        .WhereNotNull()
        .ToArray() ?? Array.Empty<string>();

    [Lazy]
    public string? Description => Details?.GetPropertyOrNull("shortDescription")?.GetStringOrNull();

    [Lazy]
    public long? ViewCount => Details?.GetPropertyOrNull("viewCount")?.GetStringOrNull()?.ParseLongOrNull();

    [Lazy]
    public string? PreviewVideoId =>
        Playability?
            .GetPropertyOrNull("errorScreen")?
            .GetPropertyOrNull("playerLegacyDesktopYpcTrailerRenderer")?
            .GetPropertyOrNull("trailerVideoId")?
            .GetStringOrNull() ??

        Playability?
            .GetPropertyOrNull("errorScreen")?
            .GetPropertyOrNull("ypcTrailerRenderer")?
            .GetPropertyOrNull("playerVars")?
            .GetStringOrNull()?
            .Pipe(UrlEx.GetQueryParameters)
            .GetValueOrDefault("video_id") ??

        Playability?
            .GetPropertyOrNull("errorScreen")?
            .GetPropertyOrNull("ypcTrailerRenderer")?
            .GetPropertyOrNull("playerResponse")?
            .GetStringOrNull()?
            // YouTube uses weird base64-like encoding here that I don't know how to deal with.
            // It's supposed to have JSON inside, but if extracted as is, it contains garbage.
            // Luckily, some of the text gets decoded correctly, which is enough for us to
            // extract the preview video ID using regex.
            .Replace('-', '+')
            .Replace('_', '/')
            .Pipe(Convert.FromBase64String)
            .Pipe(Encoding.UTF8.GetString)
            .Pipe(s => Regex.Match(s, @"video_id=(.{11})").Groups[1].Value)
            .NullIfWhiteSpace();

    [Lazy]
    private JsonElement? StreamingData => _content.GetPropertyOrNull("streamingData");

    [Lazy]
    public string? DashManifestUrl => StreamingData?.GetPropertyOrNull("dashManifestUrl")?.GetStringOrNull();

    [Lazy]
    public string? HlsManifestUrl => StreamingData?.GetPropertyOrNull("hlsManifestUrl")?.GetStringOrNull();

    [Lazy]
    public IReadOnlyList<IStreamData> Streams
    {
        get
        {
            var result = new List<IStreamData>();

            var muxedStreams = StreamingData?
                .GetPropertyOrNull("formats")?
                .EnumerateArrayOrNull()?
                .Select(j => new StreamData(j));

            if (muxedStreams is not null)
                result.AddRange(muxedStreams);

            var adaptiveStreams = StreamingData?
                .GetPropertyOrNull("adaptiveFormats")?
                .EnumerateArrayOrNull()?
                .Select(j => new StreamData(j));

            if (adaptiveStreams is not null)
                result.AddRange(adaptiveStreams);

            return result;
        }
    }

    [Lazy]
    public IReadOnlyList<ClosedCaptionTrackData> ClosedCaptionTracks => _content
        .GetPropertyOrNull("captions")?
        .GetPropertyOrNull("playerCaptionsTracklistRenderer")?
        .GetPropertyOrNull("captionTracks")?
        .EnumerateArrayOrNull()?
        .Select(j => new ClosedCaptionTrackData(j))
        .ToArray() ?? Array.Empty<ClosedCaptionTrackData>();

    public PlayerResponse(JsonElement content) => _content = content;
}

internal partial class PlayerResponse
{
    public class ClosedCaptionTrackData
    {
        private readonly JsonElement _content;

        [Lazy]
        public string? Url => _content.GetPropertyOrNull("baseUrl")?.GetStringOrNull();

        [Lazy]
        public string? LanguageCode => _content.GetPropertyOrNull("languageCode")?.GetStringOrNull();

        [Lazy]
        public string? LanguageName =>
            _content
                .GetPropertyOrNull("name")?
                .GetPropertyOrNull("simpleText")?
                .GetStringOrNull() ??

            _content
                .GetPropertyOrNull("name")?
                .GetPropertyOrNull("runs")?
                .EnumerateArrayOrNull()?
                .Select(j => j.GetPropertyOrNull("text")?.GetStringOrNull())
                .WhereNotNull()
                .ConcatToString();

        [Lazy]
        public bool IsAutoGenerated => _content
            .GetPropertyOrNull("vssId")?
            .GetStringOrNull()?
            .StartsWith("a.", StringComparison.OrdinalIgnoreCase) ?? false;

        public ClosedCaptionTrackData(JsonElement content) => _content = content;
    }
}

internal partial class PlayerResponse
{
    public class StreamData : IStreamData
    {
        private readonly JsonElement _content;

        [Lazy]
        public int? Itag => _content.GetPropertyOrNull("itag")?.GetInt32OrNull();

        [Lazy]
        private IReadOnlyDictionary<string, string>? CipherData =>
            _content
                .GetPropertyOrNull("cipher")?
                .GetStringOrNull()?
                .Pipe(UrlEx.GetQueryParameters) ??

            _content
                .GetPropertyOrNull("signatureCipher")?
                .GetStringOrNull()?
                .Pipe(UrlEx.GetQueryParameters);

        [Lazy]
        public string? Url =>
            _content.GetPropertyOrNull("url")?.GetStringOrNull() ??
            CipherData?.GetValueOrDefault("url");

        [Lazy]
        public string? Signature => CipherData?.GetValueOrDefault("s");

        [Lazy]
        public string? SignatureParameter => CipherData?.GetValueOrDefault("sp");

        [Lazy]
        public long? ContentLength =>
            _content
                .GetPropertyOrNull("contentLength")?
                .GetStringOrNull()?
                .ParseLongOrNull() ??

            Url?
                .Pipe(s => UrlEx.TryGetQueryParameterValue(s, "clen"))?
                .NullIfWhiteSpace()?
                .ParseLongOrNull();

        [Lazy]
        public long? Bitrate => _content.GetPropertyOrNull("bitrate")?.GetInt64OrNull();

        [Lazy]
        private string? MimeType => _content.GetPropertyOrNull("mimeType")?.GetStringOrNull();

        [Lazy]
        public string? Container => MimeType?.SubstringUntil(";").SubstringAfter("/");

        [Lazy]
        private bool IsAudioOnly => MimeType?.StartsWith("audio/", StringComparison.OrdinalIgnoreCase) ?? false;

        [Lazy]
        public string? Codecs => MimeType?.SubstringAfter("codecs=\"").SubstringUntil("\"");

        [Lazy]
        public string? AudioCodec => IsAudioOnly
            ? Codecs
            : Codecs?.SubstringAfter(", ").NullIfWhiteSpace();

        [Lazy]
        public string? VideoCodec
        {
            get
            {
                var codec = IsAudioOnly
                    ? null
                    : Codecs?.SubstringUntil(", ").NullIfWhiteSpace();

                // "unknown" value indicates av01 codec
                if (string.Equals(codec, "unknown", StringComparison.OrdinalIgnoreCase))
                    return "av01.0.05M.08";

                return codec;
            }
        }

        [Lazy]
        public string? VideoQualityLabel => _content.GetPropertyOrNull("qualityLabel")?.GetStringOrNull();

        [Lazy]
        public int? VideoWidth => _content.GetPropertyOrNull("width")?.GetInt32OrNull();

        [Lazy]
        public int? VideoHeight => _content.GetPropertyOrNull("height")?.GetInt32OrNull();

        [Lazy]
        public int? VideoFramerate => _content.GetPropertyOrNull("fps")?.GetInt32OrNull();

        public StreamData(JsonElement content) => _content = content;
    }
}

internal partial class PlayerResponse
{
    public static PlayerResponse Parse(string raw) => new(Json.Parse(raw));
}