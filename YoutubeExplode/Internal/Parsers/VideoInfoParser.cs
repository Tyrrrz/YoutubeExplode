using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace YoutubeExplode.Internal.Parsers
{
    internal partial class VideoInfoParser
    {
        private readonly JToken _root;

        public VideoInfoParser(JToken root)
        {
            _root = root;
        }

        public bool ParseIsAvailable() => _root.SelectToken("videoDetails") != null;

        public bool ParseIsPlayable()
        {
            var playabilityStatusValue = _root.SelectToken("playabilityStatus.status")?.Value<string>();
            return string.Equals(playabilityStatusValue, "OK", StringComparison.OrdinalIgnoreCase);
        }

        public string ParseErrorReason() => _root.SelectToken("playabilityStatus.reason")?.Value<string>();

        public string ParseAuthor() => _root.SelectToken("videoDetails.author").Value<string>();

        public string ParseTitle() => _root.SelectToken("videoDetails.title").Value<string>();

        public TimeSpan ParseDuration()
        {
            var durationSeconds = _root.SelectToken("videoDetails.lengthSeconds").Value<double>();
            return TimeSpan.FromSeconds(durationSeconds);
        }

        public IReadOnlyList<string> ParseKeywords() =>
            _root.SelectToken("videoDetails.keywords").EmptyIfNull().Values<string>().ToArray();

        public long ParseViewCount() => _root.SelectToken("videoDetails.viewCount").Value<long>();

        public string ParseDashManifestUrl()
        {
            // HACK: Don't return DASH manifest URL if it's a live stream
            // I'm not sure how to handle these streams yet
            if (ParseIsLiveStream())
                return null;

            return _root.SelectToken("streamingData.dashManifestUrl")?.Value<string>();
        }

        public string ParseHlsPlaylistUrl() => _root.SelectToken("streamingData.hlsManifestUrl")?.Value<string>();

        public TimeSpan ParseStreamInfoSetLifeSpan()
        {
            var expiresInSeconds = _root.SelectToken("streamingData.expiresInSeconds").Value<double>();
            return TimeSpan.FromSeconds(expiresInSeconds);
        }

        public bool ParseIsLiveStream() => _root.SelectToken("videoDetails.isLiveContent")?.Value<bool>() == true;

        public IEnumerable<StreamInfoParser> GetMuxedStreamInfos()
        {
            // HACK: Don't return streams if it's a live stream
            // I'm not sure how to handle these streams yet
            if (ParseIsLiveStream())
                return Enumerable.Empty<StreamInfoParser>();

            return _root.SelectToken("streamingData.formats").EmptyIfNull().Select(j => new StreamInfoParser(j));
        }

        public IEnumerable<StreamInfoParser> GetAdaptiveStreamInfos()
        {
            // HACK: Don't return streams if it's a live stream
            // I'm not sure how to handle these streams yet
            if (ParseIsLiveStream())
                return Enumerable.Empty<StreamInfoParser>();

            return _root.SelectToken("streamingData.adaptiveFormats").EmptyIfNull()
                .Select(j => new StreamInfoParser(j));
        }

        public IEnumerable<ClosedCaptionTrackInfoParser> GetClosedCaptionTrackInfos()
            => _root.SelectToken("captions.playerCaptionsTracklistRenderer.captionTracks").EmptyIfNull()
                .Select(t => new ClosedCaptionTrackInfoParser(t));
    }

    internal partial class VideoInfoParser
    {
        public class StreamInfoParser
        {
            private readonly JToken _root;

            public StreamInfoParser(JToken root)
            {
                _root = root;
            }

            public int ParseItag() => _root.SelectToken("itag").Value<int>();

            public string ParseUrl() => _root.SelectToken("url").Value<string>();

            public long ParseContentLength() => _root.SelectToken("contentLength")?.Value<long>() ?? -1;

            public long ParseBitrate() => _root.SelectToken("bitrate")?.Value<long>() ?? -1;

            public string ParseMimeType() => _root.SelectToken("mimeType").Value<string>();

            public string ParseFormat() => ParseMimeType().SubstringAfter("/").SubstringUntil(";");

            public string ParseAudioEncoding()
            {
                var codecs = ParseMimeType().SubstringAfter("codecs=\"").SubstringUntil("\"").Split(", ");
                return codecs.LastOrDefault();
            }

            public string ParseVideoEncoding()
            {
                var codecs = ParseMimeType().SubstringAfter("codecs=\"").SubstringUntil("\"").Split(", ");
                return codecs.FirstOrDefault();
            }

            public bool ParseIsAudioOnly() => _root.SelectToken("mimeType").Value<string>()
                .StartsWith("audio/", StringComparison.OrdinalIgnoreCase);

            public int ParseWidth() => _root.SelectToken("width").Value<int>();

            public int ParseHeight() => _root.SelectToken("height").Value<int>();

            public int ParseFramerate() => _root.SelectToken("fps")?.Value<int>() ?? -1;

            public string ParseQualityLabel() => _root.SelectToken("qualityLabel").Value<string>();

            public TimeSpan ParseDuration()
            {
                var durationMilliseconds = _root.SelectToken("approxDurationMs").Value<double>();
                return TimeSpan.FromMilliseconds(durationMilliseconds);
            }
        }

        public class ClosedCaptionTrackInfoParser
        {
            private readonly JToken _root;

            public ClosedCaptionTrackInfoParser(JToken root)
            {
                _root = root;
            }

            public string ParseUrl() => _root.SelectToken("baseUrl").Value<string>();

            public string ParseLanguageCode() => _root.SelectToken("languageCode").Value<string>();

            public string ParseLanguageName() => _root.SelectToken("name.simpleText").Value<string>();

            public bool ParseIsAutoGenerated() => _root.SelectToken("vssId").Value<string>()
                .StartsWith("a.", StringComparison.OrdinalIgnoreCase);
        }
    }

    internal partial class VideoInfoParser
    {
        public static VideoInfoParser Initialize(string raw)
        {
            var dic = UrlEx.SplitQuery(raw);
            var playerResponse = dic["player_response"];

            var root = JToken.Parse(playerResponse);

            return new VideoInfoParser(root);
        }
    }
}