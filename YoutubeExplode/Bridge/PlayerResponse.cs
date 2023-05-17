using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using YoutubeExplode.Utils;
using YoutubeExplode.Utils.Extensions;

namespace YoutubeExplode.Bridge;

internal partial class PlayerResponse
{
    private readonly JsonElement _content;

    private JsonElement? Playability => Memo.Cache(this, () =>
        _content.GetPropertyOrNull("playabilityStatus")
    );

    private string? PlayabilityStatus => Memo.Cache(this, () =>
        Playability?
            .GetPropertyOrNull("status")?
            .GetStringOrNull()
    );

    public string? PlayabilityError => Memo.Cache(this, () =>
        Playability?
            .GetPropertyOrNull("reason")?
            .GetStringOrNull()
    );

    public bool IsAvailable => Memo.Cache(this, () =>
        !string.Equals(PlayabilityStatus, "error", StringComparison.OrdinalIgnoreCase) &&
        Details is not null
    );

    public bool IsPlayable => Memo.Cache(this, () =>
        string.Equals(PlayabilityStatus, "ok", StringComparison.OrdinalIgnoreCase)
    );

    private JsonElement? Details => Memo.Cache(this, () =>
        _content.GetPropertyOrNull("videoDetails")
    );

    public string? Title => Memo.Cache(this, () =>
        Details?
            .GetPropertyOrNull("title")?
            .GetStringOrNull()
    );

    public string? ChannelId => Memo.Cache(this, () =>
        Details?
            .GetPropertyOrNull("channelId")?
            .GetStringOrNull()
    );

    public string? Author => Memo.Cache(this, () =>
        Details?
            .GetPropertyOrNull("author")?
            .GetStringOrNull()
    );

    public DateTimeOffset? UploadDate => Memo.Cache(this, () =>
        _content
            .GetPropertyOrNull("microformat")?
            .GetPropertyOrNull("playerMicroformatRenderer")?
            .GetPropertyOrNull("uploadDate")?
            .GetDateTimeOffset()
    );

    public TimeSpan? Duration => Memo.Cache(this, () =>
        Details?
            .GetPropertyOrNull("lengthSeconds")?
            .GetStringOrNull()?
            .ParseDoubleOrNull()?
            .Pipe(TimeSpan.FromSeconds)
    );

    public IReadOnlyList<ThumbnailData> Thumbnails => Memo.Cache(this, () =>
        Details?
            .GetPropertyOrNull("thumbnail")?
            .GetPropertyOrNull("thumbnails")?
            .EnumerateArrayOrNull()?
            .Select(j => new ThumbnailData(j))
            .ToArray() ??

        Array.Empty<ThumbnailData>()
    );

    public IReadOnlyList<string> Keywords => Memo.Cache(this, () =>
        Details?
            .GetPropertyOrNull("keywords")?
            .EnumerateArrayOrNull()?
            .Select(j => j.GetStringOrNull())
            .WhereNotNull()
            .ToArray() ??

        Array.Empty<string>()
    );

    public string? Description => Memo.Cache(this, () =>
        Details?
            .GetPropertyOrNull("shortDescription")?
            .GetStringOrNull()
    );

    public long? ViewCount => Memo.Cache(this, () =>
        Details?
            .GetPropertyOrNull("viewCount")?
            .GetStringOrNull()?
            .ParseLongOrNull()
    );

    public string? PreviewVideoId => Memo.Cache(this, () =>
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
            .NullIfWhiteSpace()
    );

    private JsonElement? StreamingData => Memo.Cache(this, () =>
        _content.GetPropertyOrNull("streamingData")
    );

    public string? DashManifestUrl => Memo.Cache(this, () =>
        StreamingData?
            .GetPropertyOrNull("dashManifestUrl")?
            .GetStringOrNull()
    );

    public string? HlsManifestUrl => Memo.Cache(this, () =>
        StreamingData?
            .GetPropertyOrNull("hlsManifestUrl")?
            .GetStringOrNull()
    );

    public IReadOnlyList<IStreamData> Streams => Memo.Cache(this, () =>
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
    });

    public IReadOnlyList<ClosedCaptionTrackData> ClosedCaptionTracks => Memo.Cache(this, () =>
        _content
            .GetPropertyOrNull("captions")?
            .GetPropertyOrNull("playerCaptionsTracklistRenderer")?
            .GetPropertyOrNull("captionTracks")?
            .EnumerateArrayOrNull()?
            .Select(j => new ClosedCaptionTrackData(j))
            .ToArray() ??

        Array.Empty<ClosedCaptionTrackData>()
    );

    public PlayerResponse(JsonElement content) => _content = content;
}

internal partial class PlayerResponse
{
    public class ClosedCaptionTrackData
    {
        private readonly JsonElement _content;

        public string? Url => Memo.Cache(this, () =>
            _content
                .GetPropertyOrNull("baseUrl")?
                .GetStringOrNull()
        );

        public string? LanguageCode => Memo.Cache(this, () =>
            _content
                .GetPropertyOrNull("languageCode")?
                .GetStringOrNull()
        );

        public string? LanguageName => Memo.Cache(this, () =>
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
                .ConcatToString()
        );

        public bool IsAutoGenerated => Memo.Cache(this, () =>
            _content
                .GetPropertyOrNull("vssId")?
                .GetStringOrNull()?
                .StartsWith("a.", StringComparison.OrdinalIgnoreCase) ?? false
        );

        public ClosedCaptionTrackData(JsonElement content) => _content = content;
    }
}

internal partial class PlayerResponse
{
    public class StreamData : IStreamData
    {
        private readonly JsonElement _content;

        public int? Itag => Memo.Cache(this, () =>
            _content
                .GetPropertyOrNull("itag")?
                .GetInt32OrNull()
        );

        private IReadOnlyDictionary<string, string>? CipherData => Memo.Cache(this, () =>
            _content
                .GetPropertyOrNull("cipher")?
                .GetStringOrNull()?
                .Pipe(UrlEx.GetQueryParameters) ??

            _content
                .GetPropertyOrNull("signatureCipher")?
                .GetStringOrNull()?
                .Pipe(UrlEx.GetQueryParameters)
        );

        public string? Url => Memo.Cache(this, () =>
            _content
                .GetPropertyOrNull("url")?
                .GetStringOrNull() ??

            CipherData?.GetValueOrDefault("url")
        );

        public string? Signature => Memo.Cache(this, () =>
            CipherData?.GetValueOrDefault("s")
        );

        public string? SignatureParameter => Memo.Cache(this, () =>
            CipherData?.GetValueOrDefault("sp")
        );

        public long? ContentLength => Memo.Cache(this, () =>
            _content
                .GetPropertyOrNull("contentLength")?
                .GetStringOrNull()?
                .ParseLongOrNull() ??

            Url?
                .Pipe(s => UrlEx.TryGetQueryParameterValue(s, "clen"))?
                .NullIfWhiteSpace()?
                .ParseLongOrNull()
        );

        public long? Bitrate => Memo.Cache(this, () =>
            _content
                .GetPropertyOrNull("bitrate")?
                .GetInt64OrNull()
        );

        private string? MimeType => Memo.Cache(this, () =>
            _content
                .GetPropertyOrNull("mimeType")?
                .GetStringOrNull()
        );

        public string? Container => Memo.Cache(this, () =>
            MimeType?
                .SubstringUntil(";")
                .SubstringAfter("/")
        );

        private bool IsAudioOnly => Memo.Cache(this, () =>
            MimeType?.StartsWith("audio/", StringComparison.OrdinalIgnoreCase) ?? false
        );

        public string? Codecs => Memo.Cache(this, () =>
            MimeType?
                .SubstringAfter("codecs=\"")
                .SubstringUntil("\"")
        );

        public string? AudioCodec => Memo.Cache(this, () =>
            IsAudioOnly
                ? Codecs
                : Codecs?.SubstringAfter(", ").NullIfWhiteSpace()
        );

        public string? VideoCodec => Memo.Cache(this, () =>
        {
            var codec = IsAudioOnly
                ? null
                : Codecs?.SubstringUntil(", ").NullIfWhiteSpace();

            // "unknown" value indicates av01 codec
            if (string.Equals(codec, "unknown", StringComparison.OrdinalIgnoreCase))
                return "av01.0.05M.08";

            return codec;
        });

        public string? VideoQualityLabel => Memo.Cache(this, () =>
            _content
                .GetPropertyOrNull("qualityLabel")?
                .GetStringOrNull()
        );

        public int? VideoWidth => Memo.Cache(this, () =>
            _content
                .GetPropertyOrNull("width")?
                .GetInt32OrNull()
        );

        public int? VideoHeight => Memo.Cache(this, () =>
            _content
                .GetPropertyOrNull("height")?
                .GetInt32OrNull()
        );

        public int? VideoFramerate => Memo.Cache(this, () =>
            _content
                .GetPropertyOrNull("fps")?
                .GetInt32OrNull()
        );

        public StreamData(JsonElement content) => _content = content;
    }
}

internal partial class PlayerResponse
{
    public static PlayerResponse Parse(string raw) => new(Json.Parse(raw));
}