using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YoutubeExplode.Exceptions;
using YoutubeExplode.Internal;
using YoutubeExplode.Internal.Extensions;

namespace YoutubeExplode.ReverseEngineering.Responses
{
    internal partial class PlaylistResponse
    {
        private readonly JsonElement _root;

        public PlaylistResponse(JsonElement root) => _root = root;

        public bool IsPlaylistAvailable() => _root
            .GetPropertyOrNull("metadata") is not null;

        public string? TryGetTitle() => _root
            .GetPropertyOrNull("metadata")?
            .GetPropertyOrNull("playlistMetadataRenderer")?
            .GetPropertyOrNull("title")?
            .GetString();

        public string? TryGetAuthor() => _root
            .GetPropertyOrNull("sidebar")?
            .GetPropertyOrNull("playlistSidebarRenderer")?
            .GetPropertyOrNull("items")?
            .EnumerateArray()
            .ElementAtOrDefault(1)
            .GetPropertyOrNull("playlistSidebarSecondaryInfoRenderer")?
            .GetPropertyOrNull("videoOwner")?
            .GetPropertyOrNull("videoOwnerRenderer")?
            .GetPropertyOrNull("title")?
            .Flatten();

        public string? TryGetDescription() => _root
            .GetPropertyOrNull("metadata")?
            .GetPropertyOrNull("playlistMetadataRenderer")?
            .GetPropertyOrNull("description")?
            .GetString();

        public long? TryGetViewCount() => _root
            .GetPropertyOrNull("sidebar")?
            .GetPropertyOrNull("playlistSidebarRenderer")?
            .GetPropertyOrNull("items")?
            .EnumerateArray()
            .FirstOrDefault()
            .GetPropertyOrNull("playlistSidebarPrimaryInfoRenderer")?
            .GetPropertyOrNull("stats")?
            .EnumerateArray()
            .ElementAtOrDefault(1)
            .GetPropertyOrNull("simpleText")?
            .GetString()?
            .StripNonDigit()
            .NullIfWhiteSpace()?
            .ParseLong();

        public string? TryGetContinuationToken() =>
            (GetVideosContent() ?? GetPlaylistVideosContent())?
            .EnumerateArray()
            .FirstOrDefault(j => j.GetPropertyOrNull("continuationItemRenderer") is not null)
            .GetPropertyOrNull("continuationItemRenderer")?
            .GetPropertyOrNull("continuationEndpoint")?
            .GetPropertyOrNull("continuationCommand")?
            .GetPropertyOrNull("token")?
            .GetString();

        public JsonElement? GetPlaylistVideosContent() =>_root
            .GetPropertyOrNull("contents")?
            .GetPropertyOrNull("twoColumnBrowseResultsRenderer")?
            .GetPropertyOrNull("tabs")?
            .EnumerateArray()
            .FirstOrDefault()
            .GetPropertyOrNull("tabRenderer")?
            .GetPropertyOrNull("content")?
            .GetPropertyOrNull("sectionListRenderer")?
            .GetPropertyOrNull("contents")?
            .EnumerateArray()
            .FirstOrDefault()
            .GetPropertyOrNull("itemSectionRenderer")?
            .GetPropertyOrNull("contents")?
            .EnumerateArray()
            .FirstOrDefault()
            .GetPropertyOrNull("playlistVideoListRenderer")?
            .GetPropertyOrNull("contents") ?? _root
            .GetPropertyOrNull("onResponseReceivedActions")?
            .EnumerateArray()
            .FirstOrDefault()
            .GetPropertyOrNull("appendContinuationItemsAction")?
            .GetPropertyOrNull("continuationItems");

        public IEnumerable<Video> GetPlaylistVideos() => Fallback.ToEmpty(
            GetPlaylistVideosContent()?
                .EnumerateArray()
                .Where(j => j.TryGetProperty("playlistVideoRenderer", out _))
                .Select(j => new Video(j.GetProperty("playlistVideoRenderer")))
        );

        public JsonElement? GetVideosContent() => _root
            .GetPropertyOrNull("contents")?
            .GetPropertyOrNull("twoColumnSearchResultsRenderer")?
            .GetPropertyOrNull("primaryContents")?
            .GetPropertyOrNull("sectionListRenderer")?
            .GetPropertyOrNull("contents") ?? _root
            .GetPropertyOrNull("onResponseReceivedCommands")?
            .EnumerateArray()
            .FirstOrDefault()
            .GetPropertyOrNull("appendContinuationItemsAction")?
            .GetPropertyOrNull("continuationItems");

        public IEnumerable<Video> GetVideos() => Fallback.ToEmpty(
            GetVideosContent()?
                .EnumerateArray()
                .FirstOrDefault()
                .GetPropertyOrNull("itemSectionRenderer")?
                .GetPropertyOrNull("contents")?
                .EnumerateArray()
                .Where(j => j.TryGetProperty("videoRenderer", out _))
                .Select(j => new Video(j.GetProperty("videoRenderer")))
        );
    }

    internal partial class PlaylistResponse
    {
        public class Video
        {
            private readonly JsonElement _root;
            private static readonly string[] _timeFormats = { @"m\:ss", @"mm\:ss", @"h\:mm\:ss", @"hh\:mm\:ss" };

            public Video(JsonElement root) => _root = root;

            public string GetId() => _root
                .GetProperty("videoId")
                .GetString();

            public string GetAuthor() => _root // Video from search results
                .GetPropertyOrNull("ownerText")?
                .Flatten() ?? _root // Video from playlist
                .GetPropertyOrNull("shortBylineText")?
                .Flatten() ?? "";

            public string GetChannelId() => _root // Video from search results
                .GetPropertyOrNull("ownerText")?
                .GetPropertyOrNull("runs")?
                .EnumerateArray()
                .FirstOrDefault()
                .GetPropertyOrNull("navigationEndpoint")?
                .GetPropertyOrNull("browseEndpoint")?
                .GetPropertyOrNull("browseId")?
                .GetString() ?? _root // Video from playlist
                .GetPropertyOrNull("shortBylineText")?
                .GetPropertyOrNull("runs")?
                .EnumerateArray()
                .FirstOrDefault()
                .GetPropertyOrNull("navigationEndpoint")?
                .GetPropertyOrNull("browseEndpoint")?
                .GetPropertyOrNull("browseId")?
                .GetString() ?? "";

            public string GetTitle() => _root
                .GetPropertyOrNull("title")?
                .Flatten() ?? "";

            // Incomplete description
            public string GetDescription() => _root
                .GetPropertyOrNull("descriptionSnippet")?
                .Flatten() ?? "";

            // Streams do not have duration
            public TimeSpan GetDuration() => _root
                .GetPropertyOrNull("lengthText")?
                .GetPropertyOrNull("simpleText")?
                .GetString()?
                .Pipe(p => TimeSpan.TryParseExact(p, _timeFormats, CultureInfo.InvariantCulture, out var duration) ? duration : default) ?? default;

            // Streams and some paid videos do not have views
            public long GetViewCount() => _root
                .GetPropertyOrNull("viewCountText")?
                .GetPropertyOrNull("simpleText")?
                .GetString()?
                .StripNonDigit()
                .NullIfWhiteSpace()?
                .ParseLong() ?? default;
        }
    }

    internal partial class PlaylistResponse
    {
        public static PlaylistResponse Parse(string raw, bool useRegex) => new(
            raw.Pipe(s => useRegex ? Regex.Match(s, @"(window\[""ytInitialData""]|var ytInitialData)\s*=\s*(.*)\s*;</script>").Groups[2].Value : s)
               .Pipe(Json.TryParse) ?? throw TransientFailureException.Generic("Playlist response is broken."));

        public static async Task<PlaylistResponse> GetAsync(YoutubeHttpClient httpClient, string id, string continuationToken = "") =>
            await Retry.WrapAsync(async () =>
            {
                var scrapePage = string.IsNullOrEmpty(continuationToken);
                string raw;

                if (scrapePage)
                {
                    var url = $"https://www.youtube.com/playlist?list={id}&hl=en&persist_hl=1";
                    raw = await httpClient.GetStringAsync(url, false);
                }
                else
                {
                    const string url = "https://www.youtube.com/youtubei/v1/browse?key=AIzaSyAO_FJ2SlqU8Q4STEHLGCilw_Y9_11qcW8";
                    var payload = BuildPayload(continuationToken: continuationToken);
                    var request = new HttpRequestMessage(HttpMethod.Post, url)
                    {
                        Content = new StringContent(payload, Encoding.UTF8, "application/json")
                    };

                    var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);

                    raw = await response.Content.ReadAsStringAsync();
                }

                var result = Parse(raw, scrapePage);

                if (scrapePage && !result.IsPlaylistAvailable())
                    throw PlaylistUnavailableException.Unavailable(id);

                return result;
            });

        public static async Task<PlaylistResponse> GetSearchResultsAsync(YoutubeHttpClient httpClient, string query, string continuationToken = "") =>
            await Retry.WrapAsync(async () =>
            {
                var queryEncoded = Uri.EscapeUriString(query);

                const string url = "https://www.youtube.com/youtubei/v1/search?key=AIzaSyAO_FJ2SlqU8Q4STEHLGCilw_Y9_11qcW8";
                var payload = BuildPayload(queryEncoded, continuationToken);
                var request = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = new StringContent(payload, Encoding.UTF8, "application/json")
                };

                var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);

                var raw = await response.Content.ReadAsStringAsync();

                return Parse(raw, false);
            });

        private static string BuildPayload(string query = "", string continuationToken = "")
        {
            string payload = @"{
    ""context"":
    {
        ""client"":
        {
            ""clientName"": ""WEB"",
            ""clientVersion"": ""2.20201220.08.00"",
            ""newVisitorCookie"": true,
            ""hl"": ""en"",
            ""gl"": ""US""
        },
        ""user"":
        {
            ""lockedSafetyMode"": false
        }
    }";

            if (!string.IsNullOrEmpty(query))
                payload += $",\n\"query\": \"{query}\"";

            if (!string.IsNullOrEmpty(continuationToken))
                payload += $",\n\"continuation\": \"{continuationToken}\"";

            payload += "}";

            return payload;
        }
    }
}
