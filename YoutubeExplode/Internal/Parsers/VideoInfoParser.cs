using System.Collections.Generic;
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

        public PlayerResponseParser GetPlayerResponse()
        {
            // Extract player response
            var playerResponseRaw = _root["player_response"];
            var playerResponseJson = JToken.Parse(playerResponseRaw);

            return new PlayerResponseParser(playerResponseJson);
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