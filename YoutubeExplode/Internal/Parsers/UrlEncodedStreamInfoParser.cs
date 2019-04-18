using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace YoutubeExplode.Internal.Parsers
{
    internal class UrlEncodedStreamInfoParser
    {
        private readonly IReadOnlyDictionary<string, string> _root;

        public UrlEncodedStreamInfoParser(IReadOnlyDictionary<string, string> root)
        {
            _root = root;
        }

        public int ParseItag() => _root["itag"].ParseInt();

        public string ParseUrl() => _root["url"];

        public string ParseSignature() => _root.GetValueOrDefault("s");

        public long ParseContentLength()
        {
            // Try to get content length from dictionary
            var contentLength = _root.GetValueOrDefault("clen").ParseLongOrDefault(-1);
            if (contentLength > 0)
                return contentLength;

            // Try to get content length from URL
            contentLength = Regex.Match(ParseUrl(), @"clen=(\d+)").Groups[1].Value.ParseLongOrDefault(-1);
            if (contentLength > 0)
                return contentLength;

            // Return -1 to signify failure
            return -1;
        }

        public long ParseBitrate() => _root["bitrate"].ParseLong();

        public string ParseMimeType() => _root["type"];

        public string ParseContainer() => ParseMimeType().SubstringUntil(";").SubstringAfter("/");

        public string ParseAudioEncoding() => ParseMimeType().SubstringAfter("codecs=\"").SubstringUntil("\"")
            .Split(", ").LastOrDefault(); // audio codec is either the only codec or the second (last) codec

        public string ParseVideoEncoding() => ParseMimeType().SubstringAfter("codecs=\"").SubstringUntil("\"")
            .Split(", ").FirstOrDefault(); // video codec is either the only codec or the first codec

        public bool ParseIsAudioOnly() => ParseMimeType().StartsWith("audio/", StringComparison.OrdinalIgnoreCase);

        public int ParseWidth() => _root["size"].SubstringUntil("x").ParseInt();

        public int ParseHeight() => _root["size"].SubstringAfter("x").ParseInt();

        public int ParseFramerate() => _root["fps"].ParseInt();

        public string ParseVideoQualityLabel() => _root["quality_label"];
    }
}