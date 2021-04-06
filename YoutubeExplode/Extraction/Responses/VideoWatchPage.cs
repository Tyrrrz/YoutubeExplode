using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using YoutubeExplode.Utils;
using YoutubeExplode.Utils.Extensions;

namespace YoutubeExplode.Extraction.Responses
{
    internal class VideoWatchPage
    {
        private readonly IHtmlDocument _root;
        private readonly Memo _memo = new();

        public VideoWatchPage(IHtmlDocument root) => _root = root;

        public bool IsValid() => _memo.Wrap(() =>
            _root.Body.QuerySelector("#player") is not null
        );

        public bool IsVideoAvailable() => _memo.Wrap(() =>
            _root.QuerySelector("meta[property=\"og:url\"]") is not null
        );

        public long? TryGetVideoLikeCount() => _memo.Wrap(() =>
            _root
                .Source
                .Text
                .Pipe(s => Regex.Match(s, @"""label""\s*:\s*""([\d,\.]+) likes""").Groups[1].Value)
                .NullIfWhiteSpace()?
                .StripNonDigit()
                .ParseLongOrNull()
        );

        public long? TryGetVideoDislikeCount() => _memo.Wrap(() =>
            _root
                .Source
                .Text
                .Pipe(s => Regex.Match(s, @"""label""\s*:\s*""([\d,\.]+) dislikes""").Groups[1].Value)
                .NullIfWhiteSpace()?
                .StripNonDigit()
                .ParseLongOrNull()
        );

        private JsonElement? TryGetPlayerConfig() => _memo.Wrap(() =>
            _root
                .GetElementsByTagName("script")
                .Select(e => e.Text())
                .Select(s => Regex.Match(s, @"ytplayer\.config\s*=\s*(\{.*\})").Groups[1].Value)
                .FirstOrDefault(s => !string.IsNullOrWhiteSpace(s))?
                .NullIfWhiteSpace()?
                .Pipe(Json.Extract)
                .Pipe(Json.TryParse)
        );

        public string? TryGetPlayerSourceUrl() => _memo.Wrap(() =>
            _root
                .GetElementsByTagName("script")
                .Select(e => e.GetAttribute("src"))
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .FirstOrDefault(s =>
                    s.Contains("player_ias", StringComparison.OrdinalIgnoreCase) &&
                    s.EndsWith(".js", StringComparison.OrdinalIgnoreCase)
                )?
                .Pipe(s => "https://youtube.com" + s) ??

            TryGetPlayerConfig()?
                .GetPropertyOrNull("assets")?
                .GetPropertyOrNull("js")?
                .GetStringOrNull()
                .Pipe(s => "https://youtube.com" + s)
        );

        public PlayerResponse? TryGetPlayerResponse() => _memo.Wrap(() =>
            _root
                .GetElementsByTagName("script")
                .Select(e => e.Text())
                .Select(s => Regex.Match(s, @"var\s+ytInitialPlayerResponse\s*=\s*(\{.*\})").Groups[1].Value)
                .FirstOrDefault(s => !string.IsNullOrWhiteSpace(s))?
                .NullIfWhiteSpace()?
                .Pipe(Json.Extract)
                .Pipe(Json.TryParse)?
                .Pipe(j => new PlayerResponse(j)) ??

            TryGetPlayerConfig()?
                .GetPropertyOrNull("args")?
                .GetPropertyOrNull("player_response")?
                .GetStringOrNull()?
                .Pipe(Json.TryParse)?
                .Pipe(j => new PlayerResponse(j))
        );

        public IReadOnlyList<IStreamInfoResponse> GetStreams() => _memo.Wrap(() =>
        {
            var result = new List<IStreamInfoResponse>();

            var playerConfig = TryGetPlayerConfig();

            var muxedStreams = playerConfig?
                .GetProperty("args")
                .GetPropertyOrNull("url_encoded_fmt_stream_map")?
                .GetStringOrNull()?
                .Split(",")
                .Select(Url.SplitQuery)
                .Select(d => new UrlEncodedStreamInfoResponse(d));

            if (muxedStreams is not null)
                result.AddRange(muxedStreams);

            var adaptiveStreams = playerConfig?
                .GetProperty("args")
                .GetPropertyOrNull("adaptive_fmts")?
                .GetStringOrNull()?
                .Split(",")
                .Select(Url.SplitQuery)
                .Select(d => new UrlEncodedStreamInfoResponse(d));

            if (adaptiveStreams is not null)
                result.AddRange(adaptiveStreams);

            return result;
        });
    }
}