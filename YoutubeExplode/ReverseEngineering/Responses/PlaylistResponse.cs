using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
            .GetPropertyOrNull("metadata") != null;

        public string? TryGetTitle() => _root
            .GetPropertyOrNull("metadata")?
            .GetPropertyOrNull("playlistMetadataRenderer")?
            .GetPropertyOrNull("title")?
            .GetString();

        // Some playlists do not have an author
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
            .ParseLong();

        public IEnumerable<Video> GetPlaylistVideos() => Fallback.ToEmpty(
                _root
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
                .GetPropertyOrNull("contents")?
                .EnumerateArray()
                .Where(j => j.TryGetProperty("playlistVideoRenderer", out _))
                .Select(j => new Video(j.GetProperty("playlistVideoRenderer")))
                );

        public IEnumerable<Video> GetVideos() => Fallback.ToEmpty(
                _root
                .GetPropertyOrNull("contents")?
                .GetPropertyOrNull("twoColumnSearchResultsRenderer")?
                .GetPropertyOrNull("primaryContents")?
                .GetPropertyOrNull("sectionListRenderer")?
                .GetPropertyOrNull("contents")?
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
                .Pipe(p => TimeSpan.ParseExact(p, _timeFormats, CultureInfo.InvariantCulture)) ?? default;

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
        public static PlaylistResponse Parse(string raw) => new(
            raw.Pipe(s => Regex.Match(s, @"(window\[""ytInitialData""]|var ytInitialData)\s*=\s*(.*)\s*;</script>").Groups[2].Value)
               .Pipe(Json.TryParse) ?? throw TransientFailureException.Generic("Playlist response is broken."));

        public static async Task<PlaylistResponse> GetAsync(YoutubeHttpClient httpClient, string id, int index = 0) =>
            await Retry.WrapAsync(async () =>
            {
                var url = $"https://www.youtube.com/playlist?list={id}&index={index}&hl=en&persist_hl=1";
                var raw = await httpClient.GetStringAsync(url, false);
                
                var result = Parse(raw);

                if (!result.IsPlaylistAvailable())
                    throw PlaylistUnavailableException.Unavailable(id);

                return result;
            });

        public static async Task<PlaylistResponse> GetSearchResultsAsync(YoutubeHttpClient httpClient, string query, int page = 0) =>
            await Retry.WrapAsync(async () =>
            {
                var queryEncoded = Uri.EscapeUriString(query);

                var url = $"https://www.youtube.com/results?search_query={queryEncoded}&hl=en&persist_hl=1";

                // Don't ensure success here but rather return an empty list
                var raw = await httpClient.GetStringAsync(url);

                return Parse(raw);
            });
    }
}
