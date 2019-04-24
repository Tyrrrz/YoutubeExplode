using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace YoutubeExplode.Internal.Parsers
{
    internal class UrlEncodedStreamInfoParser : Cached
    {
        private readonly IReadOnlyDictionary<string, string> _root;

        public UrlEncodedStreamInfoParser(IReadOnlyDictionary<string, string> root)
        {
            _root = root;
        }

        public int GetItag() => Cache(() => _root["itag"].ParseInt());

        public string GetUrl() => Cache(() => _root["url"]);

        public string TryGetSignature() => Cache(() => _root.GetValueOrDefault("s"));

        public long? TryGetContentLength() => Cache<long?>(() =>
        {
            // Try to get content length from dictionary
            var contentLength = _root.GetValueOrDefault("clen").ParseLongOrDefault(-1);
            if (contentLength > 0)
                return contentLength;

            // Try to get content length from URL
            contentLength = Regex.Match(GetUrl(), @"clen=(\d+)").Groups[1].Value.ParseLongOrDefault(-1);
            if (contentLength > 0)
                return contentLength;

            // Return null to signify failure
            return null;
        });

        public long GetBitrate() => Cache(() => _root["bitrate"].ParseLong());

        public string GetMimeType() => Cache(() => _root["type"]);

        public string GetContainer() => Cache(() => GetMimeType().SubstringUntil(";").SubstringAfter("/"));

        public string GetAudioEncoding() => Cache(() => GetMimeType().SubstringAfter("codecs=\"").SubstringUntil("\"")
            .Split(", ").LastOrDefault()); // audio codec is either the only codec or the second (last) codec

        public string GetVideoEncoding() => Cache(() => GetMimeType().SubstringAfter("codecs=\"").SubstringUntil("\"")
            .Split(", ").FirstOrDefault()); // video codec is either the only codec or the first codec

        public bool GetIsAudioOnly() => Cache(() => GetMimeType().StartsWith("audio/", StringComparison.OrdinalIgnoreCase));

        public int GetWidth() => Cache(() => _root["size"].SubstringUntil("x").ParseInt());

        public int GetHeight() => Cache(() => _root["size"].SubstringAfter("x").ParseInt());

        public int GetFramerate() => Cache(() => _root["fps"].ParseInt());

        public string GetVideoQualityLabel() => Cache(() => _root["quality_label"]);
    }
}