using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace YoutubeExplode.Internal.Parsers
{
    internal partial class SearchResultsAjaxParser
    {
        private readonly JToken _root;

        public SearchResultsAjaxParser(JToken root)
        {
            _root = root;
        }

        // Video parser is exactly the same as in playlists
        public IEnumerable<PlaylistAjaxParser.VideoParser> GetVideos()
        {
            var videosJson = _root["video"];
            return videosJson.EmptyIfNull().Select(t => new PlaylistAjaxParser.VideoParser(t));
        }
    }

    internal partial class SearchResultsAjaxParser
    {
        public static SearchResultsAjaxParser Initialize(string raw)
        {
            var root = JToken.Parse(raw);
            return new SearchResultsAjaxParser(root);
        }
    }
}