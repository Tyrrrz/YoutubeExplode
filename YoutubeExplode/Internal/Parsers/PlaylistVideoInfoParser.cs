using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace YoutubeExplode.Internal.Parsers
{
    internal class PlaylistVideoInfoParser : Cached
    {
        private readonly JToken _root;

        public PlaylistVideoInfoParser(JToken root)
        {
            _root = root;
        }

        public string GetVideoId() => Cache(() => _root.SelectToken("encrypted_id").Value<string>());

        public string GetVideoAuthor() => Cache(() => _root.SelectToken("author").Value<string>());

        public DateTimeOffset GetVideoUploadDate() => Cache(() => _root.SelectToken("added").Value<string>().ParseDateTimeOffset("M/d/yy"));

        public string GetVideoTitle() => Cache(() => _root.SelectToken("title").Value<string>());

        public string GetVideoDescription() => Cache(() => _root.SelectToken("description").Value<string>());

        public TimeSpan GetVideoDuration() => Cache(() => TimeSpan.FromSeconds(_root.SelectToken("length_seconds").Value<double>()));

        public IReadOnlyList<string> GetVideoKeywords() => Cache(() =>
        {
            var videoKeywordsJoined = _root.SelectToken("keywords").Value<string>();
            return Regex.Matches(videoKeywordsJoined, @"(?<=(^|\s)(?<q>""?))([^""]|(""""))*?(?=\<q>(?=\s|$))")
                .Cast<Match>()
                .Select(m => m.Value)
                .Where(s => !s.IsNullOrWhiteSpace())
                .ToArray();
        });

        public long GetVideoViewCount() => Cache(() => _root.SelectToken("views").Value<string>().StripNonDigit().ParseLong());

        public long GetVideoLikeCount() => Cache(() => _root.SelectToken("likes").Value<long>());

        public long GetVideoDislikeCount() => Cache(() => _root.SelectToken("dislikes").Value<long>());
    }
}