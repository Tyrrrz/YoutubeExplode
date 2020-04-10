using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YoutubeExplode.Internal;
using YoutubeExplode.Internal.Extensions;

namespace YoutubeExplode.ReverseEngineering.Responses
{
    internal partial class PlaylistResponse
    {
        private readonly JsonElement _root;

        public PlaylistResponse(JsonElement root)
        {
            _root = root;
        }

        public string GetTitle() => _root
            .GetProperty("title")
            .GetString();

        public string? TryGetAuthor() => _root
            .GetPropertyOrNull("author")?
            .GetString();

        public string? TryGetDescription() => _root
            .GetPropertyOrNull("description")?
            .GetString();

        public long? TryGetViewCount() => _root
            .GetPropertyOrNull("views")?
            .GetInt64OrNull();

        public long? TryGetLikeCount() => _root
            .GetPropertyOrNull("likes")?
            .GetInt64OrNull();

        public long? TryGetDislikeCount() => _root
            .GetPropertyOrNull("dislikes")?
            .GetInt64OrNull();

        public IEnumerable<Video> GetVideos() => _root
            .GetProperty("video")
            .EnumerateArray()
            .Select(j => new Video(j));
    }

    internal partial class PlaylistResponse
    {
        public class Video
        {
            private readonly JsonElement _root;

            public Video(JsonElement root)
            {
                _root = root;
            }

            public string GetId() => _root
                .GetProperty("encrypted_id")
                .GetString();

            public string GetAuthor() => _root
                .GetProperty("author")
                .GetString();

            public DateTimeOffset GetUploadDate() => _root
                .GetProperty("time_created")
                .GetInt64()
                .Pipe(Epoch.ToDateTimeOffset);

            public string GetTitle() => _root
                .GetProperty("title")
                .GetString();

            public string GetDescription() => _root
                .GetProperty("description")
                .GetString();

            public TimeSpan GetDuration() => _root
                .GetProperty("length_seconds")
                .GetDouble()
                .Pipe(TimeSpan.FromSeconds);

            public long GetViewCount() => _root
                .GetProperty("views")
                .GetString()
                .StripNonDigit()
                .ParseLong();

            public long GetLikeCount() => _root
                .GetProperty("likes")
                .GetInt64();

            public long GetDislikeCount() => _root
                .GetProperty("dislikes")
                .GetInt64();

            public IReadOnlyList<string> GetKeywords() => _root
                .GetProperty("keywords")
                .GetString()
                .Pipe(s => Regex.Matches(s, "\"[^\"]+\"|\\S+"))
                .Cast<Match>()
                .Select(m => m.Value)
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => s.Trim('"'))
                .ToArray();
        }
    }

    internal partial class PlaylistResponse
    {
        public static PlaylistResponse Parse(string raw) => new PlaylistResponse(Json.Parse(raw));

        public static async Task<PlaylistResponse> GetAsync(HttpClient httpClient, string id, int index = 0) =>
            await Retry.WrapAsync(async () =>
            {
                var url = $"https://youtube.com/list_ajax?style=json&action_get_list=1&list={id}&index={index}&hl=en";
                var raw = await httpClient.GetStringAsync(url);

                return Parse(raw);
            });

        public static async Task<PlaylistResponse> GetSearchResultsAsync(HttpClient httpClient, string query, int page = 0) =>
            await Retry.WrapAsync(async () =>
            {
                var queryEncoded = WebUtility.HtmlEncode(query);

                var url = $"https://youtube.com/search_ajax?style=json&search_query={queryEncoded}&page={page}&hl=en";
                var raw = await httpClient.GetStringAsync(url, false); // don't ensure success but rather return empty list

                return Parse(raw);
            });
    }
}