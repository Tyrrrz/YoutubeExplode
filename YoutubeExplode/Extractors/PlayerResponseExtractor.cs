using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using YoutubeExplode.Utils;
using YoutubeExplode.Utils.Extensions;

namespace YoutubeExplode.Extractors
{
    internal class PlayerResponseExtractor
    {
        private readonly JsonElement _content;
        private readonly Memo _memo = new();

        public PlayerResponseExtractor(JsonElement content) => _content = content;

        private string? TryGetVideoPlayabilityStatus() => _memo.Wrap(() =>
            _content
                .GetPropertyOrNull("playabilityStatus")?
                .GetPropertyOrNull("status")?
                .GetStringOrNull()
        );

        public string? TryGetVideoPlayabilityError() => _memo.Wrap(() =>
            _content
                .GetPropertyOrNull("playabilityStatus")?
                .GetPropertyOrNull("reason")?
                .GetStringOrNull()
        );

        public bool IsVideoAvailable() => _memo.Wrap(() =>
            !string.Equals(TryGetVideoPlayabilityStatus(), "error", StringComparison.OrdinalIgnoreCase)
        );

        public bool IsVideoPlayable() => _memo.Wrap(() =>
            string.Equals(TryGetVideoPlayabilityStatus(), "ok", StringComparison.OrdinalIgnoreCase)
        );

        public string? TryGetVideoTitle() => _memo.Wrap(() =>
            _content
                .GetPropertyOrNull("videoDetails")?
                .GetPropertyOrNull("title")?
                .GetStringOrNull()
        );

        public string? TryGetVideoAuthor() => _memo.Wrap(() =>
            _content
                .GetPropertyOrNull("videoDetails")?
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
            _content
                .GetPropertyOrNull("videoDetails")?
                .GetPropertyOrNull("channelId")?
                .GetStringOrNull()
        );

        public TimeSpan? TryGetVideoDuration() => _memo.Wrap(() =>
            _content
                .GetPropertyOrNull("videoDetails")?
                .GetPropertyOrNull("lengthSeconds")?
                .GetStringOrNull()?
                .ParseDoubleOrNull()?
                .Pipe(TimeSpan.FromSeconds)
        );

        public IReadOnlyList<string> GetVideoKeywords() => _memo.Wrap(() =>
            _content
                .GetPropertyOrNull("videoDetails")?
                .GetPropertyOrNull("keywords")?
                .EnumerateArrayOrEmpty()
                .Select(j => j.GetStringOrNull())
                .WhereNotNull()
                .ToArray() ??

            Array.Empty<string>()
        );

        public string? TryGetVideoDescription() => _memo.Wrap(() =>
            _content
                .GetPropertyOrNull("videoDetails")?
                .GetPropertyOrNull("shortDescription")?
                .GetStringOrNull()
        );

        public long? TryGetVideoViewCount() => _memo.Wrap(() =>
            _content
                .GetProperty("videoDetails")
                .GetPropertyOrNull("viewCount")?
                .GetStringOrNull()?
                .ParseLongOrNull()
        );

        public string? TryGetPreviewVideoId() => _memo.Wrap(() =>
            _content
                .GetPropertyOrNull("playabilityStatus")?
                .GetPropertyOrNull("errorScreen")?
                .GetPropertyOrNull("playerLegacyDesktopYpcTrailerRenderer")?
                .GetPropertyOrNull("trailerVideoId")?
                .GetStringOrNull() ??

            _content
                .GetPropertyOrNull("playabilityStatus")?
                .GetPropertyOrNull("errorScreen")?
                .GetPropertyOrNull("ypcTrailerRenderer")?
                .GetPropertyOrNull("playerVars")?
                .GetStringOrNull()?
                .Pipe(Url.SplitQuery)
                .GetValueOrDefault("video_id")
        );

        public bool IsLive() => _memo.Wrap(() =>
            _content
                .GetProperty("videoDetails")
                .GetPropertyOrNull("isLive")?
                .GetBoolean() ?? false
        );

        public string? TryGetDashManifestUrl() => _memo.Wrap(() =>
            _content
                .GetPropertyOrNull("streamingData")?
                .GetPropertyOrNull("dashManifestUrl")?
                .GetStringOrNull()
        );

        public string? TryGetHlsManifestUrl() => _memo.Wrap(() =>
            _content
                .GetPropertyOrNull("streamingData")?
                .GetPropertyOrNull("hlsManifestUrl")?
                .GetStringOrNull()
        );

        public IReadOnlyList<IStreamInfoExtractor> GetStreams() => _memo.Wrap(() =>
        {
            var result = new List<IStreamInfoExtractor>();

            var muxedStreams = _content
                .GetPropertyOrNull("streamingData")?
                .GetPropertyOrNull("formats")?
                .EnumerateArrayOrEmpty()
                .Select(j => new PlayerStreamInfoExtractor(j))
                .Where(s => !string.Equals(s.TryGetCodecs(), "unknown", StringComparison.OrdinalIgnoreCase));

            if (muxedStreams is not null)
                result.AddRange(muxedStreams);

            // TODO: unknown codecs might be av01
            // https://github.com/ytdl-org/youtube-dl/blob/162bf9e10a4e6a08f5ed156a68054ef9b4d2b60e/youtube_dl/extractor/youtube.py#L1187-L1191
            var adaptiveStreams = _content
                .GetPropertyOrNull("streamingData")?
                .GetPropertyOrNull("adaptiveFormats")?
                .EnumerateArrayOrEmpty()
                .Select(j => new PlayerStreamInfoExtractor(j))
                .Where(s => !string.Equals(s.TryGetCodecs(), "unknown", StringComparison.OrdinalIgnoreCase));

            if (adaptiveStreams is not null)
                result.AddRange(adaptiveStreams);

            return result;
        });

        public IReadOnlyList<PlayerClosedCaptionTrackInfoExtractor> GetClosedCaptionTracks() => _memo.Wrap(() =>
            _content
                .GetPropertyOrNull("captions")?
                .GetPropertyOrNull("playerCaptionsTracklistRenderer")?
                .GetPropertyOrNull("captionTracks")?
                .EnumerateArrayOrEmpty()
                .Select(j => new PlayerClosedCaptionTrackInfoExtractor(j))
                .ToArray() ??

            Array.Empty<PlayerClosedCaptionTrackInfoExtractor>()
        );
    }
}