using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using YoutubeExplode.Utils;
using YoutubeExplode.Utils.Extensions;

namespace YoutubeExplode.Bridge.Extractors
{
    internal class PlayerResponseExtractor
    {
        private readonly JsonElement _content;
        private readonly Memo _memo = new();

        public PlayerResponseExtractor(JsonElement content) => _content = content;

        private JsonElement? TryGetVideoPlayability() => _memo.Wrap(() =>
            _content.GetPropertyOrNull("playabilityStatus")
        );

        private string? TryGetVideoPlayabilityStatus() => _memo.Wrap(() =>
            TryGetVideoPlayability()?
                .GetPropertyOrNull("status")?
                .GetStringOrNull()
        );

        public string? TryGetVideoPlayabilityError() => _memo.Wrap(() =>
            TryGetVideoPlayability()?
                .GetPropertyOrNull("reason")?
                .GetStringOrNull()
        );

        public bool IsVideoAvailable() => _memo.Wrap(() =>
            !string.Equals(TryGetVideoPlayabilityStatus(), "error", StringComparison.OrdinalIgnoreCase)
        );

        public bool IsVideoPlayable() => _memo.Wrap(() =>
            string.Equals(TryGetVideoPlayabilityStatus(), "ok", StringComparison.OrdinalIgnoreCase)
        );

        private JsonElement? TryGetVideoDetails() => _memo.Wrap(() =>
            _content.GetPropertyOrNull("videoDetails")
        );

        public string? TryGetVideoTitle() => _memo.Wrap(() =>
            TryGetVideoDetails()?
                .GetPropertyOrNull("title")?
                .GetStringOrNull()
        );

        public string? TryGetVideoAuthor() => _memo.Wrap(() =>
            TryGetVideoDetails()?
                .GetPropertyOrNull("author")?
                .GetStringOrNull()
        );

        public DateTimeOffset? TryGetVideoUploadDate() => _memo.Wrap(() =>
            _content
                .GetPropertyOrNull("microformat")?
                .GetPropertyOrNull("playerMicroformatRenderer")?
                .GetPropertyOrNull("uploadDate")?
                .GetDateTimeOffset()
        );

        public string? TryGetVideoChannelId() => _memo.Wrap(() =>
            TryGetVideoDetails()?
                .GetPropertyOrNull("channelId")?
                .GetStringOrNull()
        );

        public TimeSpan? TryGetVideoDuration() => _memo.Wrap(() =>
            TryGetVideoDetails()?
                .GetPropertyOrNull("lengthSeconds")?
                .GetStringOrNull()?
                .ParseDoubleOrNull()?
                .Pipe(TimeSpan.FromSeconds)
        );

        public IReadOnlyList<ThumbnailExtractor> GetVideoThumbnails() => _memo.Wrap(() =>
            TryGetVideoDetails()?
                .GetPropertyOrNull("thumbnail")?
                .GetPropertyOrNull("thumbnails")?
                .EnumerateArrayOrNull()?
                .Select(j => new ThumbnailExtractor(j))
                .ToArray() ??

            Array.Empty<ThumbnailExtractor>()
        );

        public IReadOnlyList<string> GetVideoKeywords() => _memo.Wrap(() =>
            TryGetVideoDetails()?
                .GetPropertyOrNull("keywords")?
                .EnumerateArrayOrNull()?
                .Select(j => j.GetStringOrNull())
                .WhereNotNull()
                .ToArray() ??

            Array.Empty<string>()
        );

        public string? TryGetVideoDescription() => _memo.Wrap(() =>
            TryGetVideoDetails()?
                .GetPropertyOrNull("shortDescription")?
                .GetStringOrNull()
        );

        public long? TryGetVideoViewCount() => _memo.Wrap(() =>
            TryGetVideoDetails()?
                .GetPropertyOrNull("viewCount")?
                .GetStringOrNull()?
                .ParseLongOrNull()
        );

        public string? TryGetPreviewVideoId() => _memo.Wrap(() =>
            TryGetVideoPlayability()?
                .GetPropertyOrNull("errorScreen")?
                .GetPropertyOrNull("playerLegacyDesktopYpcTrailerRenderer")?
                .GetPropertyOrNull("trailerVideoId")?
                .GetStringOrNull() ??

            TryGetVideoPlayability()?
                .GetPropertyOrNull("errorScreen")?
                .GetPropertyOrNull("ypcTrailerRenderer")?
                .GetPropertyOrNull("playerVars")?
                .GetStringOrNull()?
                .Pipe(Url.SplitQuery)
                .GetValueOrDefault("video_id")
        );

        private JsonElement? TryGetStreamingData() => _memo.Wrap(() =>
            _content.GetPropertyOrNull("streamingData")
        );

        public string? TryGetDashManifestUrl() => _memo.Wrap(() =>
            TryGetStreamingData()?
                .GetPropertyOrNull("dashManifestUrl")?
                .GetStringOrNull()
        );

        public string? TryGetHlsManifestUrl() => _memo.Wrap(() =>
            TryGetStreamingData()?
                .GetPropertyOrNull("hlsManifestUrl")?
                .GetStringOrNull()
        );

        public IReadOnlyList<IStreamInfoExtractor> GetStreams() => _memo.Wrap(() =>
        {
            var result = new List<IStreamInfoExtractor>();

            var muxedStreams = TryGetStreamingData()?
                .GetPropertyOrNull("formats")?
                .EnumerateArrayOrNull()?
                .Select(j => new PlayerStreamInfoExtractor(j));

            if (muxedStreams is not null)
                result.AddRange(muxedStreams);

            var adaptiveStreams = TryGetStreamingData()?
                .GetPropertyOrNull("adaptiveFormats")?
                .EnumerateArrayOrNull()?
                .Select(j => new PlayerStreamInfoExtractor(j));

            if (adaptiveStreams is not null)
                result.AddRange(adaptiveStreams);

            return result;
        });

        public IReadOnlyList<PlayerClosedCaptionTrackInfoExtractor> GetClosedCaptionTracks() => _memo.Wrap(() =>
            _content
                .GetPropertyOrNull("captions")?
                .GetPropertyOrNull("playerCaptionsTracklistRenderer")?
                .GetPropertyOrNull("captionTracks")?
                .EnumerateArrayOrNull()?
                .Select(j => new PlayerClosedCaptionTrackInfoExtractor(j))
                .ToArray() ??

            Array.Empty<PlayerClosedCaptionTrackInfoExtractor>()
        );
    }
}