using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Lazy;
using YoutubeExplode.Utils;
using YoutubeExplode.Utils.Extensions;

namespace YoutubeExplode.Bridge;

internal partial class SearchResponse(JsonElement content)
{
    // Search response is incredibly inconsistent (with at least 5 variations),
    // so we employ descendant searching, which is inefficient but resilient.

    [Lazy]
    private JsonElement? ContentRoot =>
        content.GetPropertyOrNull("contents")
        ?? content.GetPropertyOrNull("onResponseReceivedCommands");

    [Lazy]
    public IReadOnlyList<VideoData> Videos =>
        ContentRoot
            ?.EnumerateDescendantProperties("videoRenderer")
            .Select(j => new VideoData(j))
            .ToArray() ?? Array.Empty<VideoData>();

    [Lazy]
    public IReadOnlyList<PlaylistData> Playlists =>
        ContentRoot
            ?.EnumerateDescendantProperties("playlistRenderer")
            .Select(j => new PlaylistData(j))
            .ToArray() ?? Array.Empty<PlaylistData>();

    [Lazy]
    public IReadOnlyList<ChannelData> Channels =>
        ContentRoot
            ?.EnumerateDescendantProperties("channelRenderer")
            .Select(j => new ChannelData(j))
            .ToArray() ?? Array.Empty<ChannelData>();

    [Lazy]
    public string? ContinuationToken =>
        ContentRoot
            ?.EnumerateDescendantProperties("continuationCommand")
            .FirstOrNull()
            ?.GetPropertyOrNull("token")
            ?.GetStringOrNull();
}

internal partial class SearchResponse
{
    internal class VideoData(JsonElement content)
    {
        [Lazy]
        public string? Id => content.GetPropertyOrNull("videoId")?.GetStringOrNull();

        [Lazy]
        public string? Title =>
            content.GetPropertyOrNull("title")?.GetPropertyOrNull("simpleText")?.GetStringOrNull()
            ?? content
                .GetPropertyOrNull("title")
                ?.GetPropertyOrNull("runs")
                ?.EnumerateArrayOrNull()
                ?.Select(j => j.GetPropertyOrNull("text")?.GetStringOrNull())
                .WhereNotNull()
                .ConcatToString();

        [Lazy]
        private JsonElement? AuthorDetails =>
            content
                .GetPropertyOrNull("longBylineText")
                ?.GetPropertyOrNull("runs")
                ?.EnumerateArrayOrNull()
                ?.ElementAtOrNull(0)
            ?? content
                .GetPropertyOrNull("shortBylineText")
                ?.GetPropertyOrNull("runs")
                ?.EnumerateArrayOrNull()
                ?.ElementAtOrNull(0);

        [Lazy]
        public string? Author => AuthorDetails?.GetPropertyOrNull("text")?.GetStringOrNull();

        [Lazy]
        public string? ChannelId =>
            AuthorDetails
                ?.GetPropertyOrNull("navigationEndpoint")
                ?.GetPropertyOrNull("browseEndpoint")
                ?.GetPropertyOrNull("browseId")
                ?.GetStringOrNull();

        [Lazy]
        public TimeSpan? Duration =>
            content
                .GetPropertyOrNull("lengthText")
                ?.GetPropertyOrNull("simpleText")
                ?.GetStringOrNull()
                ?.ParseTimeSpanOrNull(new[] { @"m\:ss", @"mm\:ss", @"h\:mm\:ss", @"hh\:mm\:ss" })
            ?? content
                .GetPropertyOrNull("lengthText")
                ?.GetPropertyOrNull("runs")
                ?.EnumerateArrayOrNull()
                ?.Select(j => j.GetPropertyOrNull("text")?.GetStringOrNull())
                .WhereNotNull()
                .ConcatToString()
                .ParseTimeSpanOrNull(new[] { @"m\:ss", @"mm\:ss", @"h\:mm\:ss", @"hh\:mm\:ss" });

        [Lazy]
        public IReadOnlyList<ThumbnailData> Thumbnails =>
            content
                .GetPropertyOrNull("thumbnail")
                ?.GetPropertyOrNull("thumbnails")
                ?.EnumerateArrayOrNull()
                ?.Select(j => new ThumbnailData(j))
                .ToArray() ?? Array.Empty<ThumbnailData>();
    }
}

internal partial class SearchResponse
{
    public class PlaylistData(JsonElement content)
    {
        [Lazy]
        public string? Id => content.GetPropertyOrNull("playlistId")?.GetStringOrNull();

        [Lazy]
        public string? Title =>
            content.GetPropertyOrNull("title")?.GetPropertyOrNull("simpleText")?.GetStringOrNull()
            ?? content
                .GetPropertyOrNull("title")
                ?.GetPropertyOrNull("runs")
                ?.EnumerateArrayOrNull()
                ?.Select(j => j.GetPropertyOrNull("text")?.GetStringOrNull())
                .WhereNotNull()
                .ConcatToString();

        [Lazy]
        private JsonElement? AuthorDetails =>
            content
                .GetPropertyOrNull("longBylineText")
                ?.GetPropertyOrNull("runs")
                ?.EnumerateArrayOrNull()
                ?.ElementAtOrNull(0);

        [Lazy]
        public string? Author => AuthorDetails?.GetPropertyOrNull("text")?.GetStringOrNull();

        [Lazy]
        public string? ChannelId =>
            AuthorDetails
                ?.GetPropertyOrNull("navigationEndpoint")
                ?.GetPropertyOrNull("browseEndpoint")
                ?.GetPropertyOrNull("browseId")
                ?.GetStringOrNull();

        [Lazy]
        public IReadOnlyList<ThumbnailData> Thumbnails =>
            content
                .GetPropertyOrNull("thumbnails")
                ?.EnumerateDescendantProperties("thumbnails")
                .SelectMany(j => j.EnumerateArrayOrEmpty())
                .Select(j => new ThumbnailData(j))
                .ToArray() ?? Array.Empty<ThumbnailData>();
    }
}

internal partial class SearchResponse
{
    public class ChannelData(JsonElement content)
    {
        [Lazy]
        public string? Id => content.GetPropertyOrNull("channelId")?.GetStringOrNull();

        [Lazy]
        public string? Title =>
            content.GetPropertyOrNull("title")?.GetPropertyOrNull("simpleText")?.GetStringOrNull()
            ?? content
                .GetPropertyOrNull("title")
                ?.GetPropertyOrNull("runs")
                ?.EnumerateArrayOrNull()
                ?.Select(j => j.GetPropertyOrNull("text")?.GetStringOrNull())
                .WhereNotNull()
                .ConcatToString();

        [Lazy]
        public IReadOnlyList<ThumbnailData> Thumbnails =>
            content
                .GetPropertyOrNull("thumbnail")
                ?.GetPropertyOrNull("thumbnails")
                ?.EnumerateArrayOrNull()
                ?.Select(j => new ThumbnailData(j))
                .ToArray() ?? Array.Empty<ThumbnailData>();

        [Lazy]
        public ulong? SubscriberCount =>
            content.GetPropertyOrNull("videoCountText")?.GetPropertyOrNull("simpleText")?.GetString()
            ?.ConvertSubscriberCountTextToNumber();
        
        static ulong? ConvertSubscriberCountTextToNumber(this string? subscriberCountText)
        {
            if (subscriberCountText == null) return null;
        
            // Split the text by spaces and take the first part which should contain the number and potential suffix.
            var parts = subscriberCountText.Split(' ');
            if (parts.Length == 0) return null;
        
            var filteredText = parts[0];
        
            // Define the multipliers for K, M, and B.
            var multipliers = new Dictionary<char, ulong>
            {
                {'K', 1_000},
                {'M', 1_000_000},
                {'B', 1_000_000_000}
            };
        
            // Check if the text ends with K, M, or B (case-insensitive) and multiply accordingly.
            char lastChar = filteredText.LastOrDefault();
            if (multipliers.TryGetValue(char.ToUpperInvariant(lastChar), out var multiplier))
            {
                filteredText = filteredText[0..^1]; // Remove the suffix
                if (decimal.TryParse(filteredText, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var number))
                {
                    return (ulong)(number * multiplier);
                }
            }
            else if (char.IsDigit(lastChar))
            {
                // If it's just a number without K, M, or B.
                if (ulong.TryParse(filteredText, out var plainNumber))
                {
                    return plainNumber;
                }
            }
        
            // If parsing fails, return null.
            return null;
        }
        
    }
}

internal partial class SearchResponse
{
    public static SearchResponse Parse(string raw) => new(Json.Parse(raw));
}
