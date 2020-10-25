using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using YoutubeExplode.Exceptions;
using YoutubeExplode.Internal;
using YoutubeExplode.Internal.Extensions;

namespace YoutubeExplode.ReverseEngineering.Responses
{
    internal partial class WatchPage
    {
        private readonly IHtmlDocument _root;

        public WatchPage(IHtmlDocument root) => _root = root;

        private bool IsOk() =>
            _root.Body.QuerySelector("#player") != null;

        public bool IsVideoAvailable() => _root
            .QuerySelector("meta[property=\"og:url\"]") != null;

        public string? TryGetPlayerSourceUrl() => _root
            .GetElementsByName("player_ias/base")
            .FirstOrDefault()?
            .GetAttribute("src")
            .Pipe(s => "https://youtube.com" + s);

        public long? TryGetVideoLikeCount() => _root
            .Source
            .Text
            .Pipe(s => Regex.Match(s, @"""label""\s*:\s*""([\d,\.]+) likes""").Groups[1].Value)
            .NullIfWhiteSpace()?
            .StripNonDigit()
            .ParseLong();

        public long? TryGetVideoDislikeCount() => _root
            .Source
            .Text
            .Pipe(s => Regex.Match(s, @"""label""\s*:\s*""([\d,\.]+) dislikes""").Groups[1].Value)
            .NullIfWhiteSpace()?
            .StripNonDigit()
            .ParseLong();

        public PlayerConfig? TryGetPlayerConfig() => _root
            .GetElementsByTagName("script")
            .Select(e => e.Text())
            .Select(s => Regex.Match(s, @"ytplayer\.config\s*=\s*(\{.*\})").Groups[1].Value)
            .FirstOrDefault(s => !string.IsNullOrWhiteSpace(s))?
            .NullIfWhiteSpace()?
            .Pipe(Json.Extract)
            .Pipe(Json.Parse)
            .Pipe(j => new PlayerConfig(j));
    }

    internal partial class WatchPage
    {
        public class PlayerConfig
        {
            private readonly JsonElement _root;

            public PlayerConfig(JsonElement root) => _root = root;

            public string GetPlayerSourceUrl() => _root
                .GetProperty("assets")
                .GetProperty("js")
                .GetString()
                .Pipe(s => "https://youtube.com" + s);

            public PlayerResponse GetPlayerResponse() => _root
                .GetProperty("args")
                .GetProperty("player_response")
                .GetString()
                .Pipe(PlayerResponse.Parse);

            private IEnumerable<VideoInfoResponse.StreamInfo> GetMuxedStreams() => Fallback.ToEmpty(
                _root
                    .GetProperty("args")
                    .GetPropertyOrNull("url_encoded_fmt_stream_map")?
                    .GetString()?
                    .Split(",")
                    .Select(Url.SplitQuery)
                    .Select(d => new VideoInfoResponse.StreamInfo(d))
            );

            private IEnumerable<VideoInfoResponse.StreamInfo> GetAdaptiveStreams() => Fallback.ToEmpty(
                _root
                    .GetProperty("args")
                    .GetPropertyOrNull("adaptive_fmts")?
                    .GetString()?
                    .Split(",")
                    .Select(Url.SplitQuery)
                    .Select(d => new VideoInfoResponse.StreamInfo(d))
            );

            public IEnumerable<VideoInfoResponse.StreamInfo> GetStreams() => GetMuxedStreams().Concat(GetAdaptiveStreams());
        }
    }

    internal partial class WatchPage
    {
        public static WatchPage Parse(string raw) => new WatchPage(Html.Parse(raw));

        public static async Task<WatchPage> GetAsync(YoutubeHttpClient httpClient, string videoId) =>
            await Retry.WrapAsync(async () =>
            {
                var url = $"https://youtube.com/watch?v={videoId}&bpctr=9999999999&hl=en";
                var raw = await httpClient.GetStringAsync(url);

                var result = Parse(raw);

                if (!result.IsOk())
                    throw TransientFailureException.Generic("Video watch page is broken.");

                if (!result.IsVideoAvailable())
                    throw VideoUnavailableException.Unavailable(videoId);

                return result;
            });
    }
}