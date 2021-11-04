using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using YoutubeExplode.Utils;
using YoutubeExplode.Utils.Extensions;

namespace YoutubeExplode.Bridge.Extractors
{
    internal class PlayerStreamInfoExtractor : IStreamInfoExtractor
    {
        private readonly JsonElement _content;
        private readonly Memo _memo = new();

        public PlayerStreamInfoExtractor(JsonElement content) => _content = content;

        public int? TryGetItag() => _memo.Wrap(() =>
            _content
                .GetPropertyOrNull("itag")?
                .GetInt32OrNull()
        );

        private IReadOnlyDictionary<string, string>? TryGetCipherData() => _memo.Wrap(() =>
            _content
                .GetPropertyOrNull("cipher")?
                .GetStringOrNull()?
                .Pipe(Url.SplitQuery) ??

            _content
                .GetPropertyOrNull("signatureCipher")?
                .GetStringOrNull()?
                .Pipe(Url.SplitQuery)
        );

        public string? TryGetUrl() => _memo.Wrap(() =>
            _content
                .GetPropertyOrNull("url")?
                .GetStringOrNull() ??

            TryGetCipherData()?.GetValueOrDefault("url")
        );

        public string? TryGetSignature() => _memo.Wrap(() =>
            TryGetCipherData()?.GetValueOrDefault("s")
        );

        public string? TryGetNSignature() => _memo.Wrap(() => TryGetUrl()?
            .Pipe(s => Regex.Match(s, @"[\?&]n=(.*?)&").Groups[1].Value));

        public string? TryGetSignatureParameter() => _memo.Wrap(() =>
            TryGetCipherData()?.GetValueOrDefault("sp")
        );

        public long? TryGetContentLength() => _memo.Wrap(() =>
            _content
                .GetPropertyOrNull("contentLength")?
                .GetStringOrNull()?
                .ParseLongOrNull() ??

            TryGetUrl()?
                .Pipe(s => Regex.Match(s, @"[\?&]clen=(\d+)").Groups[1].Value)
                .NullIfWhiteSpace()?
                .ParseLongOrNull()
        );

        public long? TryGetBitrate() => _memo.Wrap(() =>
            _content
                .GetPropertyOrNull("bitrate")?
                .GetInt64OrNull()
        );

        private string? TryGetMimeType() => _memo.Wrap(() =>
            _content
                .GetPropertyOrNull("mimeType")?
                .GetStringOrNull()
        );

        public string? TryGetContainer() => _memo.Wrap(() =>
            TryGetMimeType()?
                .SubstringUntil(";")
                .SubstringAfter("/")
        );

        private bool IsAudioOnly() => _memo.Wrap(() =>
            TryGetMimeType()?.StartsWith("audio/", StringComparison.OrdinalIgnoreCase) ?? false
        );

        public string? TryGetCodecs() => _memo.Wrap(() =>
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
            _content
                .GetPropertyOrNull("qualityLabel")?
                .GetStringOrNull()
        );

        public int? TryGetVideoWidth() => _memo.Wrap(() =>
            _content
                .GetPropertyOrNull("width")?
                .GetInt32OrNull()
        );

        public int? TryGetVideoHeight() => _memo.Wrap(() =>
            _content
                .GetPropertyOrNull("height")?
                .GetInt32OrNull()
        );

        public int? TryGetFramerate() => _memo.Wrap(() =>
            _content
                .GetPropertyOrNull("fps")?
                .GetInt32OrNull()
        );
    }
}