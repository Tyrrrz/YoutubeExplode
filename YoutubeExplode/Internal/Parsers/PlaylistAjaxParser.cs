using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace YoutubeExplode.Internal.Parsers
{
    internal partial class PlaylistAjaxParser
    {
        private readonly JToken _root;

        public PlaylistAjaxParser(JToken root)
        {
            _root = root;
        }

        public string ParseAuthor() => _root["author"]?.Value<string>() ?? ""; // system playlists don't have an author

        public string ParseTitle() => _root["title"].Value<string>();

        public string ParseDescription() => _root["description"]?.Value<string>() ?? ""; // system playlists don't have description

        public long ParseViewCount() => _root["views"]?.Value<long>() ?? 0; // watchlater does not have views

        public long ParseLikeCount() => _root["likes"]?.Value<long>() ?? 0; // system playlists don't have likes

        public long ParseDislikeCount() => _root["dislikes"]?.Value<long>() ?? 0; // system playlists don't have dislikes

        public IEnumerable<VideoParser> GetVideos()
        {
            var videosJson = _root["video"];
            return videosJson.EmptyIfNull().Select(t => new VideoParser(t));
        }
    }

    internal partial class PlaylistAjaxParser
    {
        public class VideoParser
        {
            private readonly JToken _root;

            public VideoParser(JToken root)
            {
                _root = root;
            }

            public string GetId() => _root["encrypted_id"].Value<string>();

            public string GetAuthor() => _root["author"].Value<string>();

            public DateTimeOffset GetUploadDate() => _root["added"].Value<string>().ParseDateTimeOffset("M/d/yy");

            public string GetTitle() => _root["title"].Value<string>();

            public string GetDescription() => _root["description"].Value<string>();

            public TimeSpan GetDuration() => TimeSpan.FromSeconds(_root["length_seconds"].Value<double>());

            public IReadOnlyList<string> GetKeywords()
            {
                var videoKeywordsJoined = _root["keywords"].Value<string>();
                return Regex.Matches(videoKeywordsJoined, @"(?<=(^|\s)(?<q>""?))([^""]|(""""))*?(?=\<q>(?=\s|$))")
                    .Cast<Match>()
                    .Select(m => m.Value)
                    .Where(s => s.IsNotBlank())
                    .ToArray();
            }

            public long GetViewCount() => _root["views"].Value<string>().StripNonDigit().ParseLong();

            public long GetLikeCount() => _root["likes"].Value<long>();

            public long GetDislikeCount() => _root["dislikes"].Value<long>();
        }
    }

    internal partial class PlaylistAjaxParser
    {
        public static PlaylistAjaxParser Initialize(string raw)
        {
            var root = JToken.Parse(raw);
            return new PlaylistAjaxParser(root);
        }
    }
}