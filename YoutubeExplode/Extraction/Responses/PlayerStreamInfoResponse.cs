using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.RegularExpressions;
using YoutubeExplode.Utils;
using YoutubeExplode.Utils.Extensions;

namespace YoutubeExplode.Extraction.Responses
{
    internal class PlayerStreamInfoResponse : IStreamInfoResponse
    {
        private readonly JsonElement _root;
        private readonly Memo _memo = new();

        public PlayerStreamInfoResponse(JsonElement root) => _root = root;

        public int? TryGetTag() => _memo.Wrap(() =>
            _root
                .GetPropertyOrNull("itag")?
                .GetInt32OrNull()
        );

        private IReadOnlyDictionary<string, string>? TryGetCipherData() => _memo.Wrap(() =>
            _root
                .GetPropertyOrNull("cipher")?
                .GetStringOrNull()?
                .Pipe(Url.SplitQuery) ??

            _root
                .GetPropertyOrNull("signatureCipher")?
                .GetStringOrNull()?
                .Pipe(Url.SplitQuery)
        );

        public string? TryGetUrl() => _memo.Wrap(() =>
            _root
                .GetPropertyOrNull("url")?
                .GetStringOrNull() ??

            TryGetCipherData()?.GetValueOrDefault("url")
        );

        public string? TryGetSignature() => _memo.Wrap(() =>
            TryGetCipherData()?.GetValueOrDefault("s")
        );

        public string? TryGetSignatureParameter() => _memo.Wrap(() =>
            TryGetCipherData()?.GetValueOrDefault("sp")
        );

        public long? TryGetContentLength() => _memo.Wrap(() =>
            _root
                .GetPropertyOrNull("contentLength")?
                .GetStringOrNull()?
                .ParseLongOrNull() ??

            TryGetUrl()?
                .Pipe(s => Regex.Match(s, @"[\?&]clen=(\d+)").Groups[1].Value)
                .NullIfWhiteSpace()?
                .ParseLongOrNull()
        );

        public long? TryGetBitrate() => _memo.Wrap(() =>
            _root
                .GetPropertyOrNull("bitrate")?
                .GetInt64OrNull()
        );

        private string? TryGetMimeType() => _memo.Wrap(() =>
            _root
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
            IsAudioOnly()
                ? null
                : TryGetCodecs()?.SubstringUntil(", ").NullIfWhiteSpace()
        );

        public string? TryGetVideoQualityLabel() => _memo.Wrap(() =>
            _root
                .GetPropertyOrNull("qualityLabel")?
                .GetStringOrNull()
        );

        public int? TryGetVideoWidth() => _memo.Wrap(() =>
            _root
                .GetPropertyOrNull("width")?
                .GetInt32OrNull()
        );

        public int? TryGetVideoHeight() => _memo.Wrap(() =>
            _root
                .GetPropertyOrNull("height")?
                .GetInt32OrNull()
        );

        public int? TryGetFramerate() => _memo.Wrap(() =>
            _root
                .GetPropertyOrNull("fps")?
                .GetInt32OrNull()
        );
    }
}