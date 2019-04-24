using System.Text.RegularExpressions;
using AngleSharp.Dom.Html;
using AngleSharp.Parser.Html;
using Newtonsoft.Json.Linq;

namespace YoutubeExplode.Internal.Parsers
{
    internal partial class VideoEmbedPageParser : Cached
    {
        private readonly IHtmlDocument _root;

        public VideoEmbedPageParser(IHtmlDocument root)
        {
            _root = root;
        }

        private JToken GetPlayerConfig() => Cache(() =>
        {
            var configRaw = Regex.Match(_root.Source.Text,
                    @"yt\.setConfig\({'PLAYER_CONFIG': (?<Json>\{[^\{\}]*(((?<Open>\{)[^\{\}]*)+((?<Close-Open>\})[^\{\}]*)+)*(?(Open)(?!))\})")
                .Groups["Json"].Value;
            return JToken.Parse(configRaw);
        });

        public string GetSts() => Cache(() => GetPlayerConfig().SelectToken("sts").Value<string>());

        public string GetPlayerSourceUrl() =>
            Cache(() => "https://www.youtube.com" + GetPlayerConfig().SelectToken("assets.js").Value<string>());

        public string GetChannelId() =>
            Cache(() => GetPlayerConfig().SelectToken("args.channel_path").Value<string>().SubstringAfter("channel/"));

        public string GetChannelTitle() => Cache(() => GetPlayerConfig().SelectToken("args.expanded_title").Value<string>());

        public string GetChannelLogoUrl() => Cache(() => GetPlayerConfig().SelectToken("args.profile_picture").Value<string>());
    }

    internal partial class VideoEmbedPageParser
    {
        public static VideoEmbedPageParser Initialize(string raw)
        {
            var root = new HtmlParser().Parse(raw);
            return new VideoEmbedPageParser(root);
        }
    }
}