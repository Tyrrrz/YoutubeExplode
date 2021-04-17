using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using YoutubeExplode.Utils;
using YoutubeExplode.Utils.Extensions;

namespace YoutubeExplode.Bridge.Extractors
{
    internal partial class VideoWatchPageExtractor
    {
        private readonly IHtmlDocument _content;
        private readonly Memo _memo = new();

        public VideoWatchPageExtractor(IHtmlDocument content) => _content = content;

        public bool IsVideoAvailable() => _memo.Wrap(() =>
            _content.QuerySelector("meta[property=\"og:url\"]") is not null
        );

        public long? TryGetVideoLikeCount() => _memo.Wrap(() =>
            _content
                .Source
                .Text
                .Pipe(s => Regex.Match(s, @"""label""\s*:\s*""([\d,\.]+) likes""").Groups[1].Value)
                .NullIfWhiteSpace()?
                .StripNonDigit()
                .ParseLongOrNull()
        );

        public long? TryGetVideoDislikeCount() => _memo.Wrap(() =>
            _content
                .Source
                .Text
                .Pipe(s => Regex.Match(s, @"""label""\s*:\s*""([\d,\.]+) dislikes""").Groups[1].Value)
                .NullIfWhiteSpace()?
                .StripNonDigit()
                .ParseLongOrNull()
        );

        private JsonElement? TryGetPlayerConfig() => _memo.Wrap(() =>
            _content
                .GetElementsByTagName("script")
                .Select(e => e.Text())
                .Select(s => Regex.Match(s, @"ytplayer\.config\s*=\s*(\{.*\})").Groups[1].Value)
                .FirstOrDefault(s => !string.IsNullOrWhiteSpace(s))?
                .NullIfWhiteSpace()?
                .Pipe(Json.Extract)
                .Pipe(Json.TryParse)
        );

        public string? TryGetPlayerSourceUrl() => _memo.Wrap(() =>
            _content
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

        public PlayerResponseExtractor? TryGetPlayerResponse() => _memo.Wrap(() =>
            _content
                .GetElementsByTagName("script")
                .Select(e => e.Text())
                .Select(s => Regex.Match(s, @"var\s+ytInitialPlayerResponse\s*=\s*(\{.*\})").Groups[1].Value)
                .FirstOrDefault(s => !string.IsNullOrWhiteSpace(s))?
                .NullIfWhiteSpace()?
                .Pipe(Json.Extract)
                .Pipe(Json.TryParse)?
                .Pipe(j => new PlayerResponseExtractor(j)) ??

            TryGetPlayerConfig()?
                .GetPropertyOrNull("args")?
                .GetPropertyOrNull("player_response")?
                .GetStringOrNull()?
                .Pipe(Json.TryParse)?
                .Pipe(j => new PlayerResponseExtractor(j))
        );

        public IReadOnlyList<IStreamInfoExtractor> GetStreams() => _memo.Wrap(() =>
        {
            var result = new List<IStreamInfoExtractor>();

            var playerConfig = TryGetPlayerConfig();

            var muxedStreams = playerConfig?
                .GetProperty("args")
                .GetPropertyOrNull("url_encoded_fmt_stream_map")?
                .GetStringOrNull()?
                .Split(",")
                .Select(Url.SplitQuery)
                .Select(d => new UrlEncodedStreamInfoExtractor(d));

            if (muxedStreams is not null)
                result.AddRange(muxedStreams);

            var adaptiveStreams = playerConfig?
                .GetProperty("args")
                .GetPropertyOrNull("adaptive_fmts")?
                .GetStringOrNull()?
                .Split(",")
                .Select(Url.SplitQuery)
                .Select(d => new UrlEncodedStreamInfoExtractor(d));

            if (adaptiveStreams is not null)
                result.AddRange(adaptiveStreams);

            return result;
        });
    }

    internal partial class VideoWatchPageExtractor
    {
        public static VideoWatchPageExtractor? TryCreate(string raw)
        {
            var content = Html.Parse(raw);

            var isValid = content.Body.QuerySelector("#player") is not null;
            if (!isValid)
                return null;

            return new VideoWatchPageExtractor(content);
        }
    }
}