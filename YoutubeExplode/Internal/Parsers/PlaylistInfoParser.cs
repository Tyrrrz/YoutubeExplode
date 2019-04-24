using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace YoutubeExplode.Internal.Parsers
{
    internal partial class PlaylistInfoParser : Cached
    {
        private readonly JToken _root;

        public PlaylistInfoParser(JToken root)
        {
            _root = root;
        }

        public string TryGetAuthor() => Cache(() => _root.SelectToken("author")?.Value<string>());

        public string GetTitle() => Cache(() => _root.SelectToken("title").Value<string>());

        public string TryGetDescription() => Cache(() => _root.SelectToken("description")?.Value<string>());

        public long? TryGetViewCount() => Cache(() => _root.SelectToken("views")?.Value<long>());

        public long? TryGetLikeCount() => Cache(() => _root.SelectToken("likes")?.Value<long>());

        public long? TryGetDislikeCount() => Cache(() => _root.SelectToken("dislikes")?.Value<long>());

        public IReadOnlyList<PlaylistVideoInfoParser> GetVideos() =>
            Cache(() => _root.SelectToken("video").EmptyIfNull().Select(t => new PlaylistVideoInfoParser(t)).ToArray());
    }

    internal partial class PlaylistInfoParser
    {
        public static PlaylistInfoParser Initialize(string raw)
        {
            var root = JToken.Parse(raw);
            return new PlaylistInfoParser(root);
        }
    }
}