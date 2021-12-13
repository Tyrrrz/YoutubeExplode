using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.RegularExpressions;
using YoutubeExplode.Utils;
using YoutubeExplode.Utils.Extensions;

namespace YoutubeExplode.Bridge;

internal class PlayerStreamInfoExtractor : IStreamInfoExtractor
{
    private readonly JsonElement _content;

    public PlayerStreamInfoExtractor(JsonElement content) => _content = content;

    public int? TryGetItag() => Memo.Cache(this, () =>
        _content
            .GetPropertyOrNull("itag")?
            .GetInt32OrNull()
    );

    private IReadOnlyDictionary<string, string>? TryGetCipherData() => Memo.Cache(this, () =>
        _content
            .GetPropertyOrNull("cipher")?
            .GetStringOrNull()?
            .Pipe(Url.SplitQuery) ??

        _content
            .GetPropertyOrNull("signatureCipher")?
            .GetStringOrNull()?
            .Pipe(Url.SplitQuery)
    );

    public string? TryGetUrl() => Memo.Cache(this, () =>
        _content
            .GetPropertyOrNull("url")?
            .GetStringOrNull() ??

        TryGetCipherData()?.GetValueOrDefault("url")
    );

    public string? TryGetSignature() => Memo.Cache(this, () =>
        TryGetCipherData()?.GetValueOrDefault("s")
    );

    public string? TryGetSignatureParameter() => Memo.Cache(this, () =>
        TryGetCipherData()?.GetValueOrDefault("sp")
    );

    public long? TryGetContentLength() => Memo.Cache(this, () =>
        _content
            .GetPropertyOrNull("contentLength")?
            .GetStringOrNull()?
            .ParseLongOrNull() ??

        TryGetUrl()?
            .Pipe(s => Regex.Match(s, @"[\?&]clen=(\d+)").Groups[1].Value)
            .NullIfWhiteSpace()?
            .ParseLongOrNull()
    );

    public long? TryGetBitrate() => Memo.Cache(this, () =>
        _content
            .GetPropertyOrNull("bitrate")?
            .GetInt64OrNull()
    );

    private string? TryGetMimeType() => Memo.Cache(this, () =>
        _content
            .GetPropertyOrNull("mimeType")?
            .GetStringOrNull()
    );

    public string? TryGetContainer() => Memo.Cache(this, () =>
        TryGetMimeType()?
            .SubstringUntil(";")
            .SubstringAfter("/")
    );

    private bool IsAudioOnly() => Memo.Cache(this, () =>
        TryGetMimeType()?.StartsWith("audio/", StringComparison.OrdinalIgnoreCase) ?? false
    );

    public string? TryGetCodecs() => Memo.Cache(this, () =>
        TryGetMimeType()?
            .SubstringAfter("codecs=\"")
            .SubstringUntil("\"")
    );

    public string? TryGetAudioCodec() => Memo.Cache(this, () =>
        IsAudioOnly()
            ? TryGetCodecs()
            : TryGetCodecs()?.SubstringAfter(", ").NullIfWhiteSpace()
    );

    public string? TryGetVideoCodec() => Memo.Cache(this, () =>
    {
        var codec = IsAudioOnly()
            ? null
            : TryGetCodecs()?.SubstringUntil(", ").NullIfWhiteSpace();

        // "unknown" value indicates av01 codec
        if (string.Equals(codec, "unknown", StringComparison.OrdinalIgnoreCase))
            return "av01.0.05M.08";

        return codec;
    });

    public string? TryGetVideoQualityLabel() => Memo.Cache(this, () =>
        _content
            .GetPropertyOrNull("qualityLabel")?
            .GetStringOrNull()
    );

    public int? TryGetVideoWidth() => Memo.Cache(this, () =>
        _content
            .GetPropertyOrNull("width")?
            .GetInt32OrNull()
    );

    public int? TryGetVideoHeight() => Memo.Cache(this, () =>
        _content
            .GetPropertyOrNull("height")?
            .GetInt32OrNull()
    );

    public int? TryGetFramerate() => Memo.Cache(this, () =>
        _content
            .GetPropertyOrNull("fps")?
            .GetInt32OrNull()
    );
}