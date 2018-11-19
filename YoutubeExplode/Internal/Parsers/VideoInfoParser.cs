﻿using System;
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

        public bool ParseIsSuccessful() => _root.SelectToken("videoDetails") != null;

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

        public string ParseDashManifestUrl() => _root.SelectToken("streamingData.dashManifestUrl")?.Value<string>();

        public string ParseHlsPlaylistUrl() => _root.SelectToken("hlsManifestUrl")?.Value<string>();

        public TimeSpan ParseStreamInfoSetLifeSpan()
        {
            var expiresInSeconds = _root.SelectToken("streamingData.expiresInSeconds").Value<double>();
            return TimeSpan.FromSeconds(expiresInSeconds);
        }

        public IEnumerable<StreamInfoParser> GetMuxedStreamInfos()
            => _root.SelectToken("streamingData.formats").EmptyIfNull().Select(j => new StreamInfoParser(j));

        public IEnumerable<StreamInfoParser> GetAdaptiveStreamInfos() 
            => _root.SelectToken("streamingData.adaptiveFormats").EmptyIfNull().Select(j => new StreamInfoParser(j));

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

            public int ParseItag() => _root["itag"].Value<int>();

            public string ParseUrl() => _root["url"].Value<string>();

            public long ParseContentLength() => _root["contentLength"]?.Value<long>() ?? 1; // this is a hack used for live streams in DASH manifest

            public long ParseBitrate() => _root["bitrate"].Value<long>();

            public bool ParseIsAudioOnly() => _root["mimeType"].Value<string>()
                .StartsWith("audio/", StringComparison.OrdinalIgnoreCase);

            public int ParseWidth() => _root["width"].Value<int>();

            public int ParseHeight() => _root["height"].Value<int>();

            public int ParseFramerate() => _root["fps"].Value<int>();

            public string ParseQualityLabel() => _root["qualityLabel"].Value<string>();
        }

        public class ClosedCaptionTrackInfoParser
        {
            private readonly JToken _root;

            public ClosedCaptionTrackInfoParser(JToken root)
            {
                _root = root;
            }

            public string ParseUrl() => _root["baseUrl"].Value<string>();

            public string ParseLanguageCode() => _root["languageCode"].Value<string>();

            public string ParseLanguageName() => _root["name"]["simpleText"].Value<string>();

            public bool ParseIsAutoGenerated() => _root["vssId"].Value<string>()
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