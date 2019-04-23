using System.Text.RegularExpressions;
using AngleSharp.Dom.Html;
using AngleSharp.Parser.Html;
using Newtonsoft.Json.Linq;

namespace YoutubeExplode.Internal.Decoders
{
    internal partial class VideoEmbedPageDecoder : DecoderBase
    {
        private readonly IHtmlDocument _root;

        public VideoEmbedPageDecoder(IHtmlDocument root)
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

        public string GetPlayerSourceUrl() => Cache(() =>
        {
            var relativeUrl = GetPlayerConfig().SelectToken("assets.js").Value<string>();

            if (!relativeUrl.IsNullOrWhiteSpace())
                relativeUrl = "https://www.youtube.com" + relativeUrl;

            return relativeUrl;
        });

        public string GetChannelId() =>
            Cache(() => GetPlayerConfig().SelectToken("args.channel_path").Value<string>().SubstringAfter("channel/"));

        public string GetChannelTitle() => Cache(() => GetPlayerConfig().SelectToken("args.expanded_title").Value<string>());

        public string GetChannelLogoUrl() => Cache(() => GetPlayerConfig().SelectToken("args.profile_picture").Value<string>());
    }

    internal partial class VideoEmbedPageDecoder
    {
        public static VideoEmbedPageDecoder Initialize(string raw)
        {
            var root = new HtmlParser().Parse(raw);
            return new VideoEmbedPageDecoder(root);
        }
    }
}