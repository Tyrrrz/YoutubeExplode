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

        public bool ParseIsAvailable() => !_root.GetValueOrDefault("video_id").IsNullOrWhiteSpace();

        public string ParseErrorReason() => _root.GetValueOrDefault("reason");

        public string ParseDashManifestUrl() => _root.GetValueOrDefault("dashmpd");

        public string ParseHlsManifestUrl() => _root.GetValueOrDefault("hlsvp");

        public PlayerResponseParser GetPlayerResponse()
        {
            var playerResponseRaw = _root["player_response"];
            var playerResponseJson = JToken.Parse(playerResponseRaw);

            return new PlayerResponseParser(playerResponseJson);
        }

        public IEnumerable<UrlEncodedStreamInfoParser> GetMuxedStreamInfos()
        {
            var streamInfosEncoded = _root.GetValueOrDefault("url_encoded_fmt_stream_map");

            if (streamInfosEncoded.IsNullOrWhiteSpace())
                return Enumerable.Empty<UrlEncodedStreamInfoParser>();

            return streamInfosEncoded.Split(",")
                .Select(UrlEx.SplitQuery)
                .Select(d => new UrlEncodedStreamInfoParser(d));
        }

        public IEnumerable<UrlEncodedStreamInfoParser> GetAdaptiveStreamInfos()
        {
            var streamInfosEncoded = _root.GetValueOrDefault("adaptive_fmts");

            if (streamInfosEncoded.IsNullOrWhiteSpace())
                return Enumerable.Empty<UrlEncodedStreamInfoParser>();

            return streamInfosEncoded.Split(",")
                .Select(UrlEx.SplitQuery)
                .Select(d => new UrlEncodedStreamInfoParser(d));
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