using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace YoutubeExplode.Internal.Abstractions.Wrappers
{
    internal partial class PlaylistAjax
    {
        private readonly JToken _root;

        public PlaylistAjax(JToken root)
        {
            _root = root;
        }

        public string TryGetAuthor() => _root.SelectToken("author")?.Value<string>(); // system playlists don't have an author

        public string GetTitle() => _root.SelectToken("title").Value<string>();

        public string TryGetDescription() => _root.SelectToken("description")?.Value<string>(); // system playlists don't have description

        public long? TryGetViewCount() => _root.SelectToken("views")?.Value<long>(); // watchlater does not have views

        public long? TryGetLikeCount() => _root.SelectToken("likes")?.Value<long>(); // system playlists don't have likes

        public long? TryGetDislikeCount() => _root.SelectToken("dislikes")?.Value<long>(); // system playlists don't have dislikes

        public IEnumerable<PlaylistVideo> GetVideos() => _root.SelectToken("video").EmptyIfNull().Select(t => new PlaylistVideo(t));
    }

    internal partial class PlaylistAjax
    {
        public class PlaylistVideo
        {
            private readonly JToken _root;

            public PlaylistVideo(JToken root)
            {
                _root = root;
            }

            public string GetVideoId() => _root.SelectToken("encrypted_id").Value<string>();

            public string GetVideoAuthor() => _root.SelectToken("author").Value<string>();

            public DateTimeOffset GetVideoUploadDate() => _root.SelectToken("added").Value<string>().ParseDateTimeOffset("M/d/yy");

            public string GetVideoTitle() => _root.SelectToken("title").Value<string>();

            public string GetVideoDescription() => _root.SelectToken("description").Value<string>();

            public TimeSpan GetVideoDuration()
            {
                var durationSeconds = _root.SelectToken("length_seconds").Value<double>();
                return TimeSpan.FromSeconds(durationSeconds);
            }

            public IReadOnlyList<string> GetVideoKeywords()
            {
                var videoKeywordsJoined = _root.SelectToken("keywords").Value<string>();
                return Regex.Matches(videoKeywordsJoined, @"(?<=(^|\s)(?<q>""?))([^""]|(""""))*?(?=\<q>(?=\s|$))")
                    .Cast<Match>()
                    .Select(m => m.Value)
                    .Where(s => !s.IsNullOrWhiteSpace())
                    .ToArray();
            }

            public long GetVideoViewCount() => _root.SelectToken("views").Value<string>().StripNonDigit().ParseLong();

            public long GetVideoLikeCount() => _root.SelectToken("likes").Value<long>();

            public long GetVideoDislikeCount() => _root.SelectToken("dislikes").Value<long>();
        }
    }

    internal partial class PlaylistAjax
    {
        public static PlaylistAjax Initialize(string raw)
        {
            var root = JToken.Parse(raw);
            return new PlaylistAjax(root);
        }
    }
}