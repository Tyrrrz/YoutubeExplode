using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace YoutubeExplode.Internal.Parsers
{
    internal partial class VideoInfo
    {
        private readonly IReadOnlyDictionary<string, string> _root;

        public VideoInfo(IReadOnlyDictionary<string, string> root)
        {
            _root = root;
        }

        public string GetPropertyOrDefault(string key) => _root.GetOrDefault(key);

        public int GetErrorCode() => GetPropertyOrDefault("errorcode").ParseIntOrDefault();

        public string GetErrorReason() => GetPropertyOrDefault("reason");

        public string GetTitle() => GetPropertyOrDefault("title");

        public string GetAuthor() => GetPropertyOrDefault("author");

        public TimeSpan GetDuration() => TimeSpan.FromSeconds(GetPropertyOrDefault("length_seconds").ParseDouble());

        public long GetViewCount() => GetPropertyOrDefault("view_count").ParseLong();

        public IReadOnlyList<string> GetKeywords() => GetPropertyOrDefault("keywords").Split(",");

        public string GetPreviewVideoId() => GetPropertyOrDefault("ypc_vid");

        public string GetDashManifestUrl() => GetPropertyOrDefault("dashmpd");

        public string GetHlsPlaylistUrl() => GetPropertyOrDefault("hlsvp");

        // Until a better solution is found
        public JToken GetCaptionTracksJson()
        {
            var playerResponseRaw = GetPropertyOrDefault("player_response");
            var playerResponseJson = JToken.Parse(playerResponseRaw);

            return playerResponseJson.SelectToken("$..captionTracks");
        }
    }

    internal partial class VideoInfo
    {
        public static VideoInfo Parse(string raw)
        {
            var root = UrlEx.SplitQuery(raw);
            return new VideoInfo(root);
        }
    }
}