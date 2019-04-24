using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace YoutubeExplode.Internal.Parsers
{
    internal partial class VideoInfoParser : Cached
    {
        private readonly IReadOnlyDictionary<string, string> _root;

        public VideoInfoParser(IReadOnlyDictionary<string, string> root)
        {
            _root = root;
        }

        private JToken GetPlayerResponse() => Cache(() =>
        {
            var playerResponseRaw = _root["player_response"];
            return JToken.Parse(playerResponseRaw);
        });

        public bool Validate() => Cache(() => !_root.GetValueOrDefault("video_id").IsNullOrWhiteSpace());

        public string TryGetErrorReason() => Cache(() => _root.GetValueOrDefault("reason"));

        public string TryGetPreviewVideoId() => Cache(() =>
            GetPlayerResponse().SelectToken("playabilityStatus.errorScreen.playerLegacyDesktopYpcTrailerRenderer.trailerVideoId")
                ?.Value<string>());

        public string GetVideoAuthor() => Cache(() => GetPlayerResponse().SelectToken("videoDetails.author").Value<string>());

        public string GetChannelId() => Cache(() => GetPlayerResponse().SelectToken("videoDetails.channelId").Value<string>());

        public string GetVideoTitle() => Cache(() => GetPlayerResponse().SelectToken("videoDetails.title").Value<string>());

        public TimeSpan GetVideoDuration() => Cache(() =>
            TimeSpan.FromSeconds(GetPlayerResponse().SelectToken("videoDetails.lengthSeconds").Value<double>()));

        public IReadOnlyList<string> GetVideoKeywords() =>
            Cache(() => GetPlayerResponse().SelectToken("videoDetails.keywords").EmptyIfNull().Values<string>().ToArray());

        public string TryGetDashManifestUrl() => Cache(() => _root.GetValueOrDefault("dashmpd"));

        public string TryGetHlsManifestUrl() => Cache(() => _root.GetValueOrDefault("hlsvp"));

        public TimeSpan GetExpiresIn() => Cache(() =>
            TimeSpan.FromSeconds(GetPlayerResponse().SelectToken("streamingData.expiresInSeconds").Value<double>()));

        public IReadOnlyList<UrlEncodedStreamInfoParser> GetMuxedStreamInfos() => Cache(() =>
        {
            var streamInfosEncoded = _root.GetValueOrDefault("url_encoded_fmt_stream_map");

            if (streamInfosEncoded.IsNullOrWhiteSpace())
                return new UrlEncodedStreamInfoParser[0];

            return streamInfosEncoded.Split(",")
                .Select(UrlEx.SplitQuery)
                .Select(d => new UrlEncodedStreamInfoParser(d))
                .ToArray();
        });

        public IReadOnlyList<UrlEncodedStreamInfoParser> GetAdaptiveStreamInfos() => Cache(() =>
        {
            var streamInfosEncoded = _root.GetValueOrDefault("adaptive_fmts");

            if (streamInfosEncoded.IsNullOrWhiteSpace())
                return new UrlEncodedStreamInfoParser[0];

            return streamInfosEncoded.Split(",")
                .Select(UrlEx.SplitQuery)
                .Select(d => new UrlEncodedStreamInfoParser(d))
                .ToArray();
        });

        public IEnumerable<ClosedCaptionTrackInfoParser> GetClosedCaptionTrackInfos() =>
            Cache(() => GetPlayerResponse().SelectToken("captions.playerCaptionsTracklistRenderer.captionTracks").EmptyIfNull()
                .Select(t => new ClosedCaptionTrackInfoParser(t)));
    }

    internal partial class VideoInfoParser
    {
        public static VideoInfoParser Initialize(string raw)
        {
#if NET45
            Console.WriteLine($"VideoInfo:{Environment.NewLine}{raw}{Environment.NewLine}");
#endif

            var root = UrlEx.SplitQuery(raw);
            return new VideoInfoParser(root);
        }
    }
}