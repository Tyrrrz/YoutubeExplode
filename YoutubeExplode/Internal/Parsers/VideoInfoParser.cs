﻿using System;
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

        public int GetErrorCode() => _root.GetOrDefault("errorcode").ParseIntOrDefault();

        public string GetErrorReason() => _root.GetOrDefault("reason");

        public string GetVideoId() => _root.GetOrDefault("video_id");

        public string GetTitle() => _root.GetOrDefault("title");

        public string GetAuthor() => _root.GetOrDefault("author");

        public TimeSpan GetDuration() => TimeSpan.FromSeconds(_root.GetOrDefault("length_seconds").ParseDouble());

        public long GetViewCount() => _root.GetOrDefault("view_count").ParseLong();

        public IReadOnlyList<string> GetKeywords() => _root.GetOrDefault("keywords").Split(",");

        public string GetPreviewVideoId() => _root.GetOrDefault("ypc_vid");

        public string GetDashManifestUrl() => _root.GetOrDefault("dashmpd");

        public string GetHlsPlaylistUrl() => _root.GetOrDefault("hlsvp");

        public IEnumerable<MuxedStreamInfoParser> MuxedStreamInfos()
        {
            var streamInfosEncoded = _root.GetOrDefault("url_encoded_fmt_stream_map")?.Split(",");
            return streamInfosEncoded.EmptyIfNull().Select(UrlEx.SplitQuery).Select(d => new MuxedStreamInfoParser(d));
        }

        public IEnumerable<AdaptiveStreamInfoParser> AdaptiveStreamInfos()
        {
            var streamInfosEncoded = _root.GetOrDefault("adaptive_fmts")?.Split(",");
            return streamInfosEncoded.EmptyIfNull().Select(UrlEx.SplitQuery).Select(d => new AdaptiveStreamInfoParser(d));
        }

        public IEnumerable<ClosedCaptionTrackInfoParser> ClosedCaptionTrackInfos()
        {
            var playerResponseRaw = _root.GetOrDefault("player_response");
            var playerResponseJson = JToken.Parse(playerResponseRaw);
            var closedCaptionTracksJson = playerResponseJson.SelectToken("$..captionTracks");

            return closedCaptionTracksJson.EmptyIfNull().Select(t => new ClosedCaptionTrackInfoParser(t));
        }
    }

    internal partial class VideoInfoParser
    {
        // Muxed stream info sub-parser
        public class MuxedStreamInfoParser
        {
            private readonly IReadOnlyDictionary<string, string> _root;

            public MuxedStreamInfoParser(IReadOnlyDictionary<string, string> root)
            {
                _root = root;
            }

            public string GetUrl() => _root.GetOrDefault("url");

            public int GetItag() => _root.GetOrDefault("itag").ParseInt();

            public string GetSignature() => _root.GetOrDefault("s");
        }

        // Adaptive stream info sub-parser
        public class AdaptiveStreamInfoParser
        {
            private readonly IReadOnlyDictionary<string, string> _root;

            public AdaptiveStreamInfoParser(IReadOnlyDictionary<string, string> root)
            {
                _root = root;
            }

            public string GetUrl() => _root.GetOrDefault("url");

            public int GetItag() => _root.GetOrDefault("itag").ParseInt();

            public bool GetIsAudioOnly() => _root.GetOrDefault("type")
                .StartsWith("audio/", StringComparison.OrdinalIgnoreCase);

            public string GetSignature() => _root.GetOrDefault("s");

            public long GetContentLength() => _root.GetOrDefault("clen").ParseLong();

            public long GetBitrate() => _root.GetOrDefault("bitrate").ParseLong();

            public int GetWidth() => _root.GetOrDefault("size").SubstringUntil("x").ParseInt();

            public int GetHeight() => _root.GetOrDefault("size").SubstringAfter("x").ParseInt();

            public int GetFramerate() => _root.GetOrDefault("fps").ParseInt();

            public string GetQualityLabel() => _root.GetOrDefault("quality_label");
        }

        // Closed caption track info sub-parser
        public class ClosedCaptionTrackInfoParser
        {
            private readonly JToken _root;

            public ClosedCaptionTrackInfoParser(JToken root)
            {
                _root = root;
            }

            public string GetUrl() => _root["baseUrl"].Value<string>();

            public string GetLanguageCode() => _root["languageCode"].Value<string>();

            public string GetLanguageName() => _root["name"]["simpleText"].Value<string>();

            public bool GetIsAutoGenerated() => _root["vssId"].Value<string>()
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