using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using YoutubeExplode.Internal.Abstractions.Wrappers.Shared;

namespace YoutubeExplode.Internal.Abstractions.Wrappers
{
    internal partial class VideoAjax
    {
        private readonly IReadOnlyDictionary<string, string> _root;

        public VideoAjax(IReadOnlyDictionary<string, string> root)
        {
            _root = root;
        }

        public bool Validate() => !_root.GetValueOrDefault("video_id").IsNullOrWhiteSpace();

        public string TryGetErrorReason() => _root.GetValueOrDefault("reason");

        public string TryGetDashManifestUrl() => _root.GetValueOrDefault("dashmpd");

        public string TryGetHlsManifestUrl() => _root.GetValueOrDefault("hlsvp");

        public PlayerResponse GetPlayerResponse()
        {
            var playerResponseRaw = _root["player_response"];
            var playerResponseJson = JToken.Parse(playerResponseRaw);

            return new PlayerResponse(playerResponseJson);
        }

        public IEnumerable<StreamInfoUrlEncoded> GetMuxedStreamInfos()
        {
            var streamInfosEncoded = _root.GetValueOrDefault("url_encoded_fmt_stream_map");

            if (streamInfosEncoded.IsNullOrWhiteSpace())
                return Enumerable.Empty<StreamInfoUrlEncoded>();

            return streamInfosEncoded.Split(",")
                .Select(UrlEx.SplitQuery)
                .Select(d => new StreamInfoUrlEncoded(d));
        }

        public IEnumerable<StreamInfoUrlEncoded> GetAdaptiveStreamInfos()
        {
            var streamInfosEncoded = _root.GetValueOrDefault("adaptive_fmts");

            if (streamInfosEncoded.IsNullOrWhiteSpace())
                return Enumerable.Empty<StreamInfoUrlEncoded>();

            return streamInfosEncoded.Split(",")
                .Select(UrlEx.SplitQuery)
                .Select(d => new StreamInfoUrlEncoded(d));
        }
    }

    internal partial class VideoAjax
    {
        public static VideoAjax Initialize(string raw)
        {
            var root = UrlEx.SplitQuery(raw);
            return new VideoAjax(root);
        }
    }
}