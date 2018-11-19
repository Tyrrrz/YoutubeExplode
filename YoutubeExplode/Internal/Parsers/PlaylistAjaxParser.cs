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

        public string ParseAuthor() => _root.SelectToken("author")?.Value<string>() ?? ""; // system playlists don't have an author

        public string ParseTitle() => _root.SelectToken("title").Value<string>();

        public string ParseDescription() => _root.SelectToken("description")?.Value<string>() ?? ""; // system playlists don't have description

        public long ParseViewCount() => _root.SelectToken("views")?.Value<long>() ?? 0; // watchlater does not have views

        public long ParseLikeCount() => _root.SelectToken("likes")?.Value<long>() ?? 0; // system playlists don't have likes

        public long ParseDislikeCount() => _root.SelectToken("dislikes")?.Value<long>() ?? 0; // system playlists don't have dislikes

        public IEnumerable<VideoParser> GetVideos() 
            => _root.SelectToken("video").EmptyIfNull().Select(t => new VideoParser(t));
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

            public string GetId() => _root.SelectToken("encrypted_id").Value<string>();

            public string GetAuthor() => _root.SelectToken("author").Value<string>();

            public DateTimeOffset GetUploadDate() => _root.SelectToken("added").Value<string>().ParseDateTimeOffset("M/d/yy");

            public string GetTitle() => _root.SelectToken("title").Value<string>();

            public string GetDescription() => _root.SelectToken("description").Value<string>();

            public TimeSpan GetDuration()
            {
                var durationSeconds = _root.SelectToken("length_seconds").Value<double>();
                return TimeSpan.FromSeconds(durationSeconds);
            }

            public IReadOnlyList<string> GetKeywords()
            {
                var videoKeywordsJoined = _root.SelectToken("keywords").Value<string>();
                return Regex.Matches(videoKeywordsJoined, @"(?<=(^|\s)(?<q>""?))([^""]|(""""))*?(?=\<q>(?=\s|$))")
                    .Cast<Match>()
                    .Select(m => m.Value)
                    .Where(s => s.IsNotBlank())
                    .ToArray();
            }

            public long GetViewCount() => _root.SelectToken("views").Value<string>().StripNonDigit().ParseLong();

            public long GetLikeCount() => _root.SelectToken("likes").Value<long>();

            public long GetDislikeCount() => _root.SelectToken("dislikes").Value<long>();
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