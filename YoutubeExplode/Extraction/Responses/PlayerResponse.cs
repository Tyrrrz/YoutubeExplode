using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using YoutubeExplode.Utils;
using YoutubeExplode.Utils.Extensions;

namespace YoutubeExplode.Extraction.Responses
{
    internal class PlayerResponse
    {
        private readonly JsonElement _root;
        private readonly Memo _memo = new();

        public PlayerResponse(JsonElement root) => _root = root;

        private string? TryGetVideoPlayabilityStatus() => _memo.Wrap(() =>
            _root
                .GetPropertyOrNull("playabilityStatus")?
                .GetPropertyOrNull("status")?
                .GetStringOrNull()
        );

        public string? TryGetVideoPlayabilityError() => _memo.Wrap(() =>
            _root
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
            _root
                .GetPropertyOrNull("videoDetails")?
                .GetPropertyOrNull("title")?
                .GetStringOrNull()
        );

        public string? TryGetVideoAuthor() => _memo.Wrap(() =>
            _root
                .GetPropertyOrNull("videoDetails")?
                .GetPropertyOrNull("author")?
                .GetStringOrNull()
        );

        public DateTimeOffset? TryGetVideoUploadDate() => _memo.Wrap(() =>
            _root
                .GetPropertyOrNull("microformat")?
                .GetPropertyOrNull("playerMicroformatRenderer")?
                .GetPropertyOrNull("uploadDate")?
                .GetDateTimeOffset()
        );

        public string? TryGetVideoChannelId() => _memo.Wrap(() =>
            _root
                .GetPropertyOrNull("videoDetails")?
                .GetPropertyOrNull("channelId")?
                .GetStringOrNull()
        );

        public TimeSpan? TryGetVideoDuration() => _memo.Wrap(() =>
            _root
                .GetPropertyOrNull("videoDetails")?
                .GetPropertyOrNull("lengthSeconds")?
                .GetStringOrNull()?
                .ParseDoubleOrNull()?
                .Pipe(TimeSpan.FromSeconds)
        );

        public IReadOnlyList<string> GetVideoKeywords() => _memo.Wrap(() =>
            _root
                .GetPropertyOrNull("videoDetails")?
                .GetPropertyOrNull("keywords")?
                .EnumerateArray()
                .Select(j => j.GetString())
                .ToArray() ??

            Array.Empty<string>()
        );

        public string? TryGetVideoDescription() => _memo.Wrap(() =>
            _root
                .GetPropertyOrNull("videoDetails")?
                .GetPropertyOrNull("shortDescription")?
                .GetStringOrNull()
        );

        public long? TryGetVideoViewCount() => _memo.Wrap(() =>
            _root
                .GetProperty("videoDetails")
                .GetPropertyOrNull("viewCount")?
                .GetStringOrNull()?
                .ParseLongOrNull()
        );

        public string? TryGetPreviewVideoId() => _memo.Wrap(() =>
            _root
                .GetPropertyOrNull("playabilityStatus")?
                .GetPropertyOrNull("errorScreen")?
                .GetPropertyOrNull("playerLegacyDesktopYpcTrailerRenderer")?
                .GetPropertyOrNull("trailerVideoId")?
                .GetStringOrNull() ??

            _root
                .GetPropertyOrNull("playabilityStatus")?
                .GetPropertyOrNull("errorScreen")?
                .GetPropertyOrNull("ypcTrailerRenderer")?
                .GetPropertyOrNull("playerVars")?
                .GetStringOrNull()?
                .Pipe(Url.SplitQuery)
                .GetValueOrDefault("video_id")
        );

        public bool IsLive() => _memo.Wrap(() =>
            _root
                .GetProperty("videoDetails")
                .GetPropertyOrNull("isLive")?
                .GetBoolean() ?? false
        );

        public string? TryGetDashManifestUrl() => _memo.Wrap(() =>
            _root
                .GetPropertyOrNull("streamingData")?
                .GetPropertyOrNull("dashManifestUrl")?
                .GetStringOrNull()
        );

        public string? TryGetHlsManifestUrl() => _memo.Wrap(() =>
            _root
                .GetPropertyOrNull("streamingData")?
                .GetPropertyOrNull("hlsManifestUrl")?
                .GetStringOrNull()
        );

        public IReadOnlyList<IStreamInfoResponse> GetStreams() => _memo.Wrap(() =>
        {
            var result = new List<IStreamInfoResponse>();

            var muxedStreams = _root
                .GetPropertyOrNull("streamingData")?
                .GetPropertyOrNull("formats")?
                .EnumerateArray()
                .Select(j => new PlayerStreamInfoResponse(j))
                .Where(s => !string.Equals(s.TryGetCodecs(), "unknown", StringComparison.OrdinalIgnoreCase));

            if (muxedStreams is not null)
                result.AddRange(muxedStreams);

            // TODO: unknown codecs might be av01
            // https://github.com/ytdl-org/youtube-dl/blob/162bf9e10a4e6a08f5ed156a68054ef9b4d2b60e/youtube_dl/extractor/youtube.py#L1187-L1191
            var adaptiveStreams = _root
                .GetPropertyOrNull("streamingData")?
                .GetPropertyOrNull("adaptiveFormats")?
                .EnumerateArray()
                .Select(j => new PlayerStreamInfoResponse(j))
                .Where(s => !string.Equals(s.TryGetCodecs(), "unknown", StringComparison.OrdinalIgnoreCase));

            if (adaptiveStreams is not null)
                result.AddRange(adaptiveStreams);

            return result;
        });

        public IReadOnlyList<PlayerClosedCaptionTrackInfoResponse> GetClosedCaptionTracks() => _memo.Wrap(() =>
            _root
                .GetPropertyOrNull("captions")?
                .GetPropertyOrNull("playerCaptionsTracklistRenderer")?
                .GetPropertyOrNull("captionTracks")?
                .EnumerateArray()
                .Select(j => new PlayerClosedCaptionTrackInfoResponse(j))
                .ToArray() ??

            Array.Empty<PlayerClosedCaptionTrackInfoResponse>()
        );
    }
}