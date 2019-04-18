using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace YoutubeExplode.Internal.Parsers
{
    internal partial class VideoInfoParser
    {
        private readonly IReadOnlyDictionary<string, string> _root;

        public VideoInfoParser(IReadOnlyDictionary<string, string> root)
        {
            _root = root;
        }

        public string ParseId() => _root.GetValueOrDefault("video_id");

        public string ParseErrorReason() => _root.GetValueOrDefault("reason");

        public string ParsePreviewVideoId() => _root.GetValueOrDefault("ypc_vid");

        public string ParseDashManifestUrl() => _root.GetValueOrDefault("dashmpd");

        public string ParseHlsManifestUrl() => _root.GetValueOrDefault("hlsvp");

        public PlayerResponseParser GetPlayerResponse()
        {
            var playerResponseRaw = _root["player_response"];
            var playerResponseJson = JToken.Parse(playerResponseRaw);

            return new PlayerResponseParser(playerResponseJson);
        }

        public IEnumerable<MuxedStreamInfoParser> GetMuxedStreamInfos()
        {
            var streamInfosEncoded = _root.GetValueOrDefault("url_encoded_fmt_stream_map");

            if (streamInfosEncoded.IsNullOrWhiteSpace())
                return Enumerable.Empty<MuxedStreamInfoParser>();

            return streamInfosEncoded.Split(",")
                .Select(UrlEx.SplitQuery)
                .Select(d => new MuxedStreamInfoParser(d));
        }

        public IEnumerable<AdaptiveStreamInfoParser> GetAdaptiveStreamInfos()
        {
            var streamInfosEncoded = _root.GetValueOrDefault("adaptive_fmts");

            if (streamInfosEncoded.IsNullOrWhiteSpace())
                return Enumerable.Empty<AdaptiveStreamInfoParser>();

            return streamInfosEncoded.Split(",")
                .Select(UrlEx.SplitQuery)
                .Select(d => new AdaptiveStreamInfoParser(d));
        }
    }

    internal partial class VideoInfoParser
    {
        public class MuxedStreamInfoParser
        {
            private readonly IReadOnlyDictionary<string, string> _root;

            public MuxedStreamInfoParser(IReadOnlyDictionary<string, string> root)
            {
                _root = root;
            }

            public int ParseItag() => _root["itag"].ParseInt();

            public string ParseUrl() => _root["url"];

            public string ParseSignature() => _root.GetValueOrDefault("s");

            public long ParseContentLength() => Regex.Match(ParseUrl(), @"clen=(\d+)").Groups[1].Value.ParseLongOrDefault(-1);

            public string ParseMimeType() => _root["type"];

            public string ParseContainer() => ParseMimeType().SubstringUntil(";").SubstringAfter("/");

            public string ParseAudioEncoding() => ParseMimeType().SubstringAfter("codecs=\"").SubstringUntil("\"")
                .Split(", ").LastOrDefault(); // audio codec is either the only codec or the second (last) codec

            public string ParseVideoEncoding() => ParseMimeType().SubstringAfter("codecs=\"").SubstringUntil("\"")
                .Split(", ").FirstOrDefault(); // video codec is either the only codec or the first codec
        }

        public class AdaptiveStreamInfoParser
        {
            private readonly IReadOnlyDictionary<string, string> _root;

            public AdaptiveStreamInfoParser(IReadOnlyDictionary<string, string> root)
            {
                _root = root;
            }

            public int ParseItag() => _root["itag"].ParseInt();

            public string ParseUrl() => _root["url"];

            public string ParseSignature() => _root.GetValueOrDefault("s");

            public long ParseContentLength() => _root["clen"].ParseLong();

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

    internal partial class VideoInfoParser
    {
        public static VideoInfoParser Initialize(string raw)
        {
            var root = UrlEx.SplitQuery(raw);
            return new VideoInfoParser(root);
        }
    }
}