using System;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using YoutubeExplode.Utils;
using YoutubeExplode.Utils.Extensions;

namespace YoutubeExplode.Extraction.Responses
{
    // TODO: delete?
    internal partial class EmbedPage
    {
        private readonly IHtmlDocument _root;

        public EmbedPage(IHtmlDocument root) => _root = root;

        public string? TryGetPlayerSourceUrl() => _root
            .GetElementsByTagName("script")
            .Select(e => e.GetAttribute("src"))
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .FirstOrDefault(s =>
                s.Contains("player_ias", StringComparison.OrdinalIgnoreCase) &&
                s.EndsWith(".js", StringComparison.OrdinalIgnoreCase)
            )?
            .Pipe(s => "https://youtube.com" + s);

        public PlayerConfig? TryGetPlayerConfig() =>
            _root
                .GetElementsByTagName("script")
                .Select(e => e.Text())
                .Select(s => Regex.Match(s, @"['""]PLAYER_CONFIG['""]\s*:\s*(\{.*\})").Groups[1].Value)
                .FirstOrDefault(s => !string.IsNullOrWhiteSpace(s))?
                .NullIfWhiteSpace()?
                .Pipe(Json.Extract)
                .Pipe(Json.Parse)
                .Pipe(j => new PlayerConfig(j)) ??

            _root
                .GetElementsByTagName("script")
                .Select(e => e.Text())
                .Select(s => Regex.Match(s, @"yt.setConfig\((\{.*\})").Groups[1].Value)
                .FirstOrDefault(s => !string.IsNullOrWhiteSpace(s))?
                .NullIfWhiteSpace()?
                .Pipe(Json.Extract)
                .Pipe(Json.Parse)
                .Pipe(j => new PlayerConfig(j)) ??

            _root
                .GetElementsByTagName("script")
                .Select(e => e.Text())
                .Select(s => Regex.Match(s, @"ytcfg.set\((\{.*\})").Groups[1].Value)
                .FirstOrDefault(s => !string.IsNullOrWhiteSpace(s))?
                .NullIfWhiteSpace()?
                .Pipe(Json.Extract)
                .Pipe(Json.Parse)
                .Pipe(j => new PlayerConfig(j));
    }

    internal partial class EmbedPage
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
        }
    }
}