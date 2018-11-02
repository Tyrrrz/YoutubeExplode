using System;
using System.Collections.Generic;
using System.Linq;
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

        public int ParseErrorCode() => _root.GetOrDefault("errorcode").ParseIntOrDefault();

        public string ParseErrorReason() => _root.GetOrDefault("reason");

        public string ParsePreviewVideoId() => _root.GetOrDefault("ypc_vid");

        public string ParseId() => _root.GetOrDefault("video_id");

        public string ParseAuthor() => _root.GetOrDefault("author");

        public string ParseTitle() => _root.GetOrDefault("title");

        public TimeSpan ParseDuration() => TimeSpan.FromSeconds(_root.GetOrDefault("length_seconds").ParseDouble());

        public IReadOnlyList<string> ParseKeywords()
        {
            var playerResponseRaw = _root.GetOrDefault("player_response");
            var playerResponseJson = JToken.Parse(playerResponseRaw);
            var keywordsJson = playerResponseJson["videoDetails"]["keywords"];

            // If no keywords - return empty
            if (keywordsJson == null)
                return new string[0];

            return keywordsJson.Values<string>().ToArray();
        }

        public long ParseViewCount()
        {
            var playerResponseRaw = _root.GetOrDefault("player_response");
            var playerResponseJson = JToken.Parse(playerResponseRaw);

            return playerResponseJson["videoDetails"]["viewCount"].Value<string>().ParseLong();
        }

        public string ParseDashManifestUrl() => _root.GetOrDefault("dashmpd");

        public string ParseHlsPlaylistUrl() => _root.GetOrDefault("hlsvp");

        public IEnumerable<MuxedStreamInfoParser> GetMuxedStreamInfos()
        {
            var streamInfosEncoded = _root.GetOrDefault("url_encoded_fmt_stream_map");

            if (streamInfosEncoded.IsBlank())
                return Enumerable.Empty<MuxedStreamInfoParser>();

            return streamInfosEncoded.Split(",")
                .Select(UrlEx.SplitQuery)
                .Select(d => new MuxedStreamInfoParser(d));
        }

        public IEnumerable<AdaptiveStreamInfoParser> GetAdaptiveStreamInfos()
        {
            var streamInfosEncoded = _root.GetOrDefault("adaptive_fmts");

            if (streamInfosEncoded.IsBlank())
                return Enumerable.Empty<AdaptiveStreamInfoParser>();

            return streamInfosEncoded.Split(",")
                .Select(UrlEx.SplitQuery)
                .Select(d => new AdaptiveStreamInfoParser(d));
        }

        public IEnumerable<ClosedCaptionTrackInfoParser> GetClosedCaptionTrackInfos()
        {
            var playerResponseRaw = _root.GetOrDefault("player_response");
            var playerResponseJson = JToken.Parse(playerResponseRaw);
            var closedCaptionTracksJson = playerResponseJson.SelectToken("$..captionTracks");

            return closedCaptionTracksJson.EmptyIfNull().Select(t => new ClosedCaptionTrackInfoParser(t));
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

            public int ParseItag() => _root.GetOrDefault("itag").ParseInt();

            public string ParseUrl() => _root.GetOrDefault("url");

            public string ParseSignature() => _root.GetOrDefault("s");
        }

        public class AdaptiveStreamInfoParser
        {
            private readonly IReadOnlyDictionary<string, string> _root;

            public AdaptiveStreamInfoParser(IReadOnlyDictionary<string, string> root)
            {
                _root = root;
            }

            public int ParseItag() => _root.GetOrDefault("itag").ParseInt();

            public string ParseUrl() => _root.GetOrDefault("url");

            public string ParseSignature() => _root.GetOrDefault("s");

            public long ParseContentLength() => _root.GetOrDefault("clen").ParseLong();

            public long ParseBitrate() => _root.GetOrDefault("bitrate").ParseLong();

            public bool ParseIsAudioOnly() => _root.GetOrDefault("type")
                .StartsWith("audio/", StringComparison.OrdinalIgnoreCase);

            public int ParseWidth() => _root.GetOrDefault("size").SubstringUntil("x").ParseInt();

            public int ParseHeight() => _root.GetOrDefault("size").SubstringAfter("x").ParseInt();

            public int ParseFramerate() => _root.GetOrDefault("fps").ParseInt();

            public string ParseQualityLabel() => _root.GetOrDefault("quality_label");
        }

        public class ClosedCaptionTrackInfoParser
        {
            private readonly JToken _root;

            public ClosedCaptionTrackInfoParser(JToken root)
            {
                _root = root;
            }

            public string ParseUrl() => _root["baseUrl"].Value<string>();

            public string ParseLanguageCode() => _root["languageCode"].Value<string>();

            public string ParseLanguageName() => _root["name"]["simpleText"].Value<string>();

            public bool ParseIsAutoGenerated() => _root["vssId"].Value<string>()
                .StartsWith("a.", StringComparison.OrdinalIgnoreCase);
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