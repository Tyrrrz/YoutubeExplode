using System.Text.RegularExpressions;
using AngleSharp.Dom.Html;
using AngleSharp.Parser.Html;
using Newtonsoft.Json.Linq;
using YoutubeExplode.Internal.Abstractions.Wrappers.Shared;

namespace YoutubeExplode.Internal.Abstractions.Wrappers
{
    internal partial class VideoEmbedPage
    {
        private readonly IHtmlDocument _root;

        public VideoEmbedPage(IHtmlDocument root)
        {
            _root = root;
        }

        public PlayerConfig GetPlayerConfig()
        {
            var configRaw = Regex.Match(_root.Source.Text,
                    @"yt\.setConfig\({'PLAYER_CONFIG': (?<Json>\{[^\{\}]*(((?<Open>\{)[^\{\}]*)+((?<Close-Open>\})[^\{\}]*)+)*(?(Open)(?!))\})")
                .Groups["Json"].Value;
            var configJson = JToken.Parse(configRaw);

            return new PlayerConfig(configJson);
        }
    }

    internal partial class VideoEmbedPage
    {
        public static VideoEmbedPage Initialize(string raw)
        {
            var root = new HtmlParser().Parse(raw);
            return new VideoEmbedPage(root);
        }
    }
}