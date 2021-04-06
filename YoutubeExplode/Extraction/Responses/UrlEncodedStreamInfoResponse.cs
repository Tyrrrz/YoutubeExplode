using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using YoutubeExplode.Utils;
using YoutubeExplode.Utils.Extensions;

namespace YoutubeExplode.Extraction.Responses
{
    internal class UrlEncodedStreamInfoResponse : IStreamInfoResponse
    {
        private readonly IReadOnlyDictionary<string, string> _root;
        private readonly Memo _memo = new();

        public UrlEncodedStreamInfoResponse(IReadOnlyDictionary<string, string> root) =>
            _root = root;

        public int? TryGetTag() => _memo.Wrap(() =>
            _root.GetValueOrDefault("itag")?.ParseIntOrNull()
        );

        public string? TryGetUrl() => _memo.Wrap(()
            => _root.GetValueOrDefault("url")
        );

        public string? TryGetSignature() => _memo.Wrap(() =>
            _root.GetValueOrDefault("s")
        );

        public string? TryGetSignatureParameter() => _memo.Wrap(() =>
            _root.GetValueOrDefault("sp")
        );

        public long? TryGetContentLength() => _memo.Wrap(() =>
            _root
                .GetValueOrDefault("clen")?
                .ParseLongOrNull() ??

            TryGetUrl()?
                .Pipe(s => Regex.Match(s, @"clen=(\d+)").Groups[1].Value)
                .NullIfWhiteSpace()?
                .ParseLongOrNull()
        );

        public long? TryGetBitrate() => _memo.Wrap(() =>
            _root.GetValueOrDefault("bitrate")?.ParseLongOrNull()
        );

        private string? TryGetMimeType() => _memo.Wrap(() => _root.GetValueOrDefault("type"));

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
            IsAudioOnly()
                ? null
                : TryGetCodecs()?.SubstringUntil(", ").NullIfWhiteSpace()
        );

        public string? TryGetVideoQualityLabel() => _memo.Wrap(() =>
            _root.GetValueOrDefault("quality_label")
        );

        private string? TryGetVideoResolution() => _memo.Wrap(() =>
            _root.GetValueOrDefault("size")
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
            _root
                .GetValueOrDefault("fps")?
                .ParseIntOrNull()
        );
    }
}