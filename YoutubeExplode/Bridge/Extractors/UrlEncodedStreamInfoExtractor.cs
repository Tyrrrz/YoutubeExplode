using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using YoutubeExplode.Utils;
using YoutubeExplode.Utils.Extensions;

namespace YoutubeExplode.Bridge.Extractors;

internal class UrlEncodedStreamInfoExtractor : IStreamInfoExtractor
{
    private readonly IReadOnlyDictionary<string, string> _content;
    private readonly Memo _memo = new();

    public UrlEncodedStreamInfoExtractor(IReadOnlyDictionary<string, string> content) =>
        _content = content;

    public int? TryGetItag() => _memo.Wrap(() =>
        _content.GetValueOrDefault("itag")?.ParseIntOrNull()
    );

    public string? TryGetUrl() => _memo.Wrap(()
        => _content.GetValueOrDefault("url")
    );

    public string? TryGetSignature() => _memo.Wrap(() =>
        _content.GetValueOrDefault("s")
    );

    public string? TryGetSignatureParameter() => _memo.Wrap(() =>
        _content.GetValueOrDefault("sp")
    );

    public long? TryGetContentLength() => _memo.Wrap(() =>
        _content
            .GetValueOrDefault("clen")?
            .ParseLongOrNull() ??

        TryGetUrl()?
            .Pipe(s => Regex.Match(s, @"clen=(\d+)").Groups[1].Value)
            .NullIfWhiteSpace()?
            .ParseLongOrNull()
    );

    public long? TryGetBitrate() => _memo.Wrap(() =>
        _content.GetValueOrDefault("bitrate")?.ParseLongOrNull()
    );

    private string? TryGetMimeType() => _memo.Wrap(() => _content.GetValueOrDefault("type"));

    public string? TryGetContainer() => _memo.Wrap(() =>
        TryGetMimeType()?
            .SubstringUntil(";")
            .SubstringAfter("/")
    );

    private bool IsAudioOnly() => _memo.Wrap(() =>
        TryGetMimeType()?.StartsWith("audio/", StringComparison.OrdinalIgnoreCase) ?? false
    );

    private string? TryGetCodecs() => _memo.Wrap(() =>
        TryGetMimeType()?
            .SubstringAfter("codecs=\"")
            .SubstringUntil("\"")
    );

    public string? TryGetAudioCodec() => _memo.Wrap(() =>
        IsAudioOnly()
            ? TryGetCodecs()
            : TryGetCodecs()?.SubstringAfter(", ").NullIfWhiteSpace()
    );

    public string? TryGetVideoCodec() => _memo.Wrap(() =>
    {
        var codec = IsAudioOnly()
            ? null
            : TryGetCodecs()?.SubstringUntil(", ").NullIfWhiteSpace();

        // "unknown" value indicates av01 codec
        if (string.Equals(codec, "unknown", StringComparison.OrdinalIgnoreCase))
            return "av01.0.05M.08";

        return codec;
    });

    public string? TryGetVideoQualityLabel() => _memo.Wrap(() =>
        _content.GetValueOrDefault("quality_label")
    );

    private string? TryGetVideoResolution() => _memo.Wrap(() =>
        _content.GetValueOrDefault("size")
    );

    public int? TryGetVideoWidth() => _memo.Wrap(() =>
        TryGetVideoResolution()?
            .SubstringUntil("x")
            .NullIfWhiteSpace()?
            .ParseIntOrNull()
    );

    public int? TryGetVideoHeight() => _memo.Wrap(() =>
        TryGetVideoResolution()?
            .SubstringAfter("x")
            .NullIfWhiteSpace()?
            .ParseIntOrNull()
    );

    public int? TryGetFramerate() => _memo.Wrap(() =>
        _content
            .GetValueOrDefault("fps")?
            .ParseIntOrNull()
    );
}