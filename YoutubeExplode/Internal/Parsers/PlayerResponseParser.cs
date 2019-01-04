using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace YoutubeExplode.Internal.Parsers
{
    internal partial class PlayerResponseParser
    {
        private readonly JToken _root;

        public PlayerResponseParser(JToken root)
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

        public string ParseChannelId() => _root.SelectToken("videoDetails.channelId").Value<string>();

        public string ParseTitle() => _root.SelectToken("videoDetails.title").Value<string>();

        public TimeSpan ParseDuration()
        {
            var durationSeconds = _root.SelectToken("videoDetails.lengthSeconds").Value<double>();
            return TimeSpan.FromSeconds(durationSeconds);
        }

        public IReadOnlyList<string> ParseKeywords() =>
            _root.SelectToken("videoDetails.keywords").EmptyIfNull().Values<string>().ToArray();

        public bool ParseIsLiveStream() => _root.SelectToken("videoDetails.isLive")?.Value<bool>() == true;

        public string ParseDashManifestUrl()
        {
            // HACK: Don't return DASH manifest URL if it's a live stream
            // I'm not sure how to handle these streams yet
            if (ParseIsLiveStream())
                return null;

            return _root.SelectToken("streamingData.dashManifestUrl")?.Value<string>();
        }

        public string ParseHlsManifestUrl() => _root.SelectToken("streamingData.hlsManifestUrl")?.Value<string>();

        public TimeSpan ParseStreamInfoSetExpiresIn()
        {
            var expiresInSeconds = _root.SelectToken("streamingData.expiresInSeconds").Value<double>();
            return TimeSpan.FromSeconds(expiresInSeconds);
        }

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

    internal partial class PlayerResponseParser
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

            public long ParseBitrate() => _root.SelectToken("bitrate").Value<long>();

            public string ParseMimeType() => _root.SelectToken("mimeType").Value<string>();

            public string ParseContainer() => ParseMimeType().SubstringUntil(";").SubstringAfter("/");

            public string ParseAudioEncoding() => ParseMimeType().SubstringAfter("codecs=\"").SubstringUntil("\"")
                .Split(", ").LastOrDefault(); // audio codec is either the only codec or the second (last) codec

            public string ParseVideoEncoding() => ParseMimeType().SubstringAfter("codecs=\"").SubstringUntil("\"")
                .Split(", ").FirstOrDefault(); // video codec is either the only codec or the first codec

            public bool ParseIsAudioOnly() => ParseMimeType().StartsWith("audio/", StringComparison.OrdinalIgnoreCase);

            public int ParseWidth() => _root.SelectToken("width").Value<int>();

            public int ParseHeight() => _root.SelectToken("height").Value<int>();

            public int ParseFramerate() => _root.SelectToken("fps").Value<int>();

            public string ParseVideoQualityLabel() => _root.SelectToken("qualityLabel").Value<string>();
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
}