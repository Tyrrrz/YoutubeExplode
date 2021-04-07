using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using YoutubeExplode.Utils;
using YoutubeExplode.Utils.Extensions;

namespace YoutubeExplode.Extractors
{
    internal partial class PlaylistExtractor
    {
        private readonly JsonElement _content;

        public PlaylistExtractor(JsonElement content) => _content = content;

        public bool IsPlaylistAvailable() => _content
            .GetPropertyOrNull("metadata") is not null;

        public string? TryGetTitle() => _content
            .GetPropertyOrNull("metadata")?
            .GetPropertyOrNull("playlistMetadataRenderer")?
            .GetPropertyOrNull("title")?
            .GetStringOrNull();

        public string? TryGetAuthor() => _content
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

        public string? TryGetDescription() => _content
            .GetPropertyOrNull("metadata")?
            .GetPropertyOrNull("playlistMetadataRenderer")?
            .GetPropertyOrNull("description")?
            .GetStringOrNull();

        public long? TryGetViewCount() => _content
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
            .GetStringOrNull()?
            .StripNonDigit()
            .NullIfWhiteSpace()?
            .ParseLongOrNull();

        public string? TryGetContinuationToken() =>
            (GetVideosContent() ?? GetPlaylistVideosContent())?
            .EnumerateArray()
            .FirstOrDefault(j => j.GetPropertyOrNull("continuationItemRenderer") is not null)
            .GetPropertyOrNull("continuationItemRenderer")?
            .GetPropertyOrNull("continuationEndpoint")?
            .GetPropertyOrNull("continuationCommand")?
            .GetPropertyOrNull("token")?
            .GetStringOrNull();

        public JsonElement? GetPlaylistVideosContent() =>_content
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
            .GetPropertyOrNull("contents") ?? _content
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

        public JsonElement? GetVideosContent() => _content
            .GetPropertyOrNull("contents")?
            .GetPropertyOrNull("twoColumnSearchResultsRenderer")?
            .GetPropertyOrNull("primaryContents")?
            .GetPropertyOrNull("sectionListRenderer")?
            .GetPropertyOrNull("contents") ?? _content
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

    internal partial class PlaylistExtractor
    {
        public class Video
        {
            private readonly JsonElement _content;
            private static readonly string[] _timeFormats = { @"m\:ss", @"mm\:ss", @"h\:mm\:ss", @"hh\:mm\:ss" };

            public Video(JsonElement root) => _content = root;

            public string GetId() => _content
                .GetProperty("videoId")
                .GetString()!;

            public string GetAuthor() => _content // Video from search results
                .GetPropertyOrNull("ownerText")?
                .Flatten() ?? _content // Video from playlist
                .GetPropertyOrNull("shortBylineText")?
                .Flatten() ?? "";

            public string GetChannelId() => _content // Video from search results
                .GetPropertyOrNull("ownerText")?
                .GetPropertyOrNull("runs")?
                .EnumerateArray()
                .FirstOrDefault()
                .GetPropertyOrNull("navigationEndpoint")?
                .GetPropertyOrNull("browseEndpoint")?
                .GetPropertyOrNull("browseId")?
                .GetStringOrNull() ?? _content // Video from playlist
                .GetPropertyOrNull("shortBylineText")?
                .GetPropertyOrNull("runs")?
                .EnumerateArray()
                .FirstOrDefault()
                .GetPropertyOrNull("navigationEndpoint")?
                .GetPropertyOrNull("browseEndpoint")?
                .GetPropertyOrNull("browseId")?
                .GetStringOrNull() ?? "";

            public string GetTitle() => _content
                .GetPropertyOrNull("title")?
                .Flatten() ?? "";

            // Incomplete description
            public string GetDescription() => _content
                .GetPropertyOrNull("descriptionSnippet")?
                .Flatten() ?? "";

            // Streams do not have duration
            public TimeSpan GetDuration() => _content
                .GetPropertyOrNull("lengthText")?
                .GetPropertyOrNull("simpleText")?
                .GetStringOrNull()?
                .Pipe(p => TimeSpan.TryParseExact(p, _timeFormats, CultureInfo.InvariantCulture, out var duration) ? duration : default) ?? default;

            // Streams and some paid videos do not have views
            public long GetViewCount() => _content
                .GetPropertyOrNull("viewCountText")?
                .GetPropertyOrNull("simpleText")?
                .GetStringOrNull()?
                .StripNonDigit()
                .NullIfWhiteSpace()?
                .ParseLongOrNull() ?? default;
        }
    }
}