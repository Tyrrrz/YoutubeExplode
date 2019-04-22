using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace YoutubeExplode.Internal.Abstractions.Wrappers.Shared
{
    internal class StreamInfoUrlEncoded
    {
        private readonly IReadOnlyDictionary<string, string> _root;

        public StreamInfoUrlEncoded(IReadOnlyDictionary<string, string> root)
        {
            _root = root;
        }

        public int GetItag() => _root["itag"].ParseInt();

        public string GetUrl() => _root["url"];

        public string TryGetSignature() => _root.GetValueOrDefault("s");

        public long? TryGetContentLength()
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
        }

        public long GetBitrate() => _root["bitrate"].ParseLong();

        public string GetMimeType() => _root["type"];

        public string GetContainer() => GetMimeType().SubstringUntil(";").SubstringAfter("/");

        public string GetAudioEncoding() => GetMimeType().SubstringAfter("codecs=\"").SubstringUntil("\"")
            .Split(", ").LastOrDefault(); // audio codec is either the only codec or the second (last) codec

        public string GetVideoEncoding() => GetMimeType().SubstringAfter("codecs=\"").SubstringUntil("\"")
            .Split(", ").FirstOrDefault(); // video codec is either the only codec or the first codec

        public bool GetIsAudioOnly() => GetMimeType().StartsWith("audio/", StringComparison.OrdinalIgnoreCase);

        public int GetWidth() => _root["size"].SubstringUntil("x").ParseInt();

        public int GetHeight() => _root["size"].SubstringAfter("x").ParseInt();

        public int GetFramerate() => _root["fps"].ParseInt();

        public string GetVideoQualityLabel() => _root["quality_label"];
    }
}