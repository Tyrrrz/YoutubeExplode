using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using YoutubeExplode.Internal;
using YoutubeExplode.Internal.Extensions;

namespace YoutubeExplode.ReverseEngineering.Responses
{
    internal partial class PlayerResponse
    {
        private readonly JsonElement _root;

        public PlayerResponse(JsonElement root) => _root = root;

        private string GetVideoPlayabilityStatus() => _root
            .GetProperty("playabilityStatus")
            .GetProperty("status")
            .GetString();

        public string? TryGetVideoPlayabilityError() => _root
            .GetPropertyOrNull("playabilityStatus")?
            .GetPropertyOrNull("reason")?
            .GetString();

        public bool IsVideoAvailable() =>
            !string.Equals(GetVideoPlayabilityStatus(), "error", StringComparison.OrdinalIgnoreCase);

        public bool IsVideoPlayable() =>
            string.Equals(GetVideoPlayabilityStatus(), "ok", StringComparison.OrdinalIgnoreCase);

        public string GetVideoTitle() => _root
            .GetProperty("videoDetails")
            .GetProperty("title")
            .GetString();

        public string GetVideoAuthor() => _root
            .GetProperty("videoDetails")
            .GetProperty("author")
            .GetString();

        public DateTimeOffset GetVideoUploadDate() => _root
            .GetProperty("microformat")
            .GetProperty("playerMicroformatRenderer")
            .GetProperty("uploadDate")
            .GetDateTimeOffset();

        public string GetVideoChannelId() => _root
            .GetProperty("videoDetails")
            .GetProperty("channelId")
            .GetString();

        public TimeSpan GetVideoDuration() => _root
            .GetProperty("videoDetails")
            .GetProperty("lengthSeconds")
            .GetString()
            .ParseDouble()
            .Pipe(TimeSpan.FromSeconds);

        public IReadOnlyList<string> GetVideoKeywords() => Fallback.ToEmpty(
            _root
                .GetProperty("videoDetails")
                .GetPropertyOrNull("keywords")?
                .EnumerateArray()
                .Select(j => j.GetString())
                .ToArray()
        );

        public string GetVideoDescription() => _root
            .GetProperty("videoDetails")
            .GetProperty("shortDescription")
            .GetString();

        public long? TryGetVideoViewCount() => _root
            .GetProperty("videoDetails")
            .GetPropertyOrNull("viewCount")?
            .GetString()
            .ParseLong();

        public string? TryGetPreviewVideoId() =>
            _root
                .GetPropertyOrNull("playabilityStatus")?
                .GetPropertyOrNull("errorScreen")?
                .GetPropertyOrNull("playerLegacyDesktopYpcTrailerRenderer")?
                .GetPropertyOrNull("trailerVideoId")?
                .GetString() ??
            _root
                .GetPropertyOrNull("playabilityStatus")?
                .GetPropertyOrNull("errorScreen")?
                .GetPropertyOrNull("ypcTrailerRenderer")?
                .GetPropertyOrNull("playerVars")?
                .GetString()
                .Pipe(Url.SplitQuery)
                .GetValueOrDefault("video_id");

        public bool IsLive() => _root
            .GetProperty("videoDetails")
            .GetPropertyOrNull("isLive")?
            .GetBoolean() ?? false;

        public string? TryGetDashManifestUrl() => _root
            .GetPropertyOrNull("streamingData")?
            .GetPropertyOrNull("dashManifestUrl")?
            .GetString();

        public string? TryGetHlsManifestUrl() =>_root
            .GetPropertyOrNull("streamingData")?
            .GetPropertyOrNull("hlsManifestUrl")?
            .GetString();

        private IEnumerable<StreamInfo> GetMuxedStreams() => Fallback.ToEmpty(
            _root
                .GetPropertyOrNull("streamingData")?
                .GetPropertyOrNull("formats")?
                .EnumerateArray()
                .Select(j => new StreamInfo(j))
                .Where(s => !string.Equals(s.GetCodecs(), "unknown", StringComparison.OrdinalIgnoreCase))
        );

        private IEnumerable<StreamInfo> GetAdaptiveStreams() => Fallback.ToEmpty(
            _root
                .GetPropertyOrNull("streamingData")?
                .GetPropertyOrNull("adaptiveFormats")?
                .EnumerateArray()
                .Select(j => new StreamInfo(j))
                .Where(s => !string.Equals(s.GetCodecs(), "unknown", StringComparison.OrdinalIgnoreCase))
        );

        public IEnumerable<StreamInfo> GetStreams() => GetMuxedStreams().Concat(GetAdaptiveStreams());

        public IEnumerable<ClosedCaptionTrack> GetClosedCaptionTracks() => Fallback.ToEmpty(
            _root
                .GetPropertyOrNull("captions")?
                .GetPropertyOrNull("playerCaptionsTracklistRenderer")?
                .GetPropertyOrNull("captionTracks")?
                .EnumerateArray()
                .Select(j => new ClosedCaptionTrack(j))
        );
    }

    internal partial class PlayerResponse
    {
        public class StreamInfo : IStreamInfoProvider
        {
            private readonly JsonElement _root;

            public StreamInfo(JsonElement root) => _root = root;

            public int GetTag() => _root
                .GetProperty("itag")
                .GetInt32();

            public string GetUrl() =>
                _root
                    .GetPropertyOrNull("url")?
                    .GetString() ??
                _root
                    .GetPropertyOrNull("cipher")?
                    .GetString()
                    .Pipe(Url.SplitQuery)["url"] ??
                _root
                    .GetProperty("signatureCipher")
                    .GetString()
                    .Pipe(Url.SplitQuery)["url"];

            public string? TryGetSignature() =>
                _root
                    .GetPropertyOrNull("cipher")?
                    .GetString()
                    .Pipe(Url.SplitQuery)
                    .GetValueOrDefault("s") ??
                _root
                    .GetPropertyOrNull("signatureCipher")?
                    .GetString()
                    .Pipe(Url.SplitQuery)
                    .GetValueOrDefault("s");

            public string? TryGetSignatureParameter() =>
                _root
                    .GetPropertyOrNull("cipher")?
                    .GetString()
                    .Pipe(Url.SplitQuery)
                    .GetValueOrDefault("sp") ??
                _root
                    .GetPropertyOrNull("signatureCipher")?
                    .GetString()
                    .Pipe(Url.SplitQuery)
                    .GetValueOrDefault("sp");

            public long? TryGetContentLength() =>
                _root
                    .GetPropertyOrNull("contentLength")?
                    .GetString()
                    .ParseLong() ??
                GetUrl()
                    .Pipe(s => Regex.Match(s, @"[\?&]clen=(\d+)").Groups[1].Value)
                    .NullIfWhiteSpace()?
                    .ParseLong();

            public long GetBitrate() => _root
                .GetProperty("bitrate")
                .GetInt64();

            private string GetMimeType() => _root
                .GetProperty("mimeType")
                .GetString();

            public string GetContainer() => GetMimeType()
                .SubstringUntil(";")
                .SubstringAfter("/");

            private bool IsAudioOnly() => GetMimeType()
                .StartsWith("audio/", StringComparison.OrdinalIgnoreCase);

            public string GetCodecs() => GetMimeType()
                .SubstringAfter("codecs=\"")
                .SubstringUntil("\"");

            public string? TryGetAudioCodec() =>
                IsAudioOnly()
                    ? GetCodecs()
                    : GetCodecs().SubstringAfter(", ").NullIfWhiteSpace();

            public string? TryGetVideoCodec() =>
                IsAudioOnly()
                    ? null
                    : GetCodecs().SubstringUntil(", ").NullIfWhiteSpace();

            public string? TryGetVideoQualityLabel() => _root
                .GetPropertyOrNull("qualityLabel")?
                .GetString();

            public int? TryGetVideoWidth() => _root
                .GetPropertyOrNull("width")?
                .GetInt32();

            public int? TryGetVideoHeight() => _root
                .GetPropertyOrNull("height")?
                .GetInt32();

            public int? TryGetFramerate() => _root
                .GetPropertyOrNull("fps")?
                .GetInt32();
        }

        public class ClosedCaptionTrack
        {
            private readonly JsonElement _root;

            public ClosedCaptionTrack(JsonElement root) => _root = root;

            public string GetUrl() => _root
                .GetProperty("baseUrl")
                .GetString();

            public string GetLanguageCode() => _root
                .GetProperty("languageCode")
                .GetString();

            public string GetLanguageName() => _root
                .GetProperty("name")
                .GetProperty("simpleText")
                .GetString();

            public bool IsAutoGenerated() => _root
                .GetProperty("vssId")
                .GetString()
                .StartsWith("a.", StringComparison.OrdinalIgnoreCase);
        }
    }

    internal partial class PlayerResponse
    {
        public static PlayerResponse Parse(string raw) => new PlayerResponse(Json.Parse(raw));
    }
}