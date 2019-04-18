using System.Text.RegularExpressions;
using AngleSharp.Dom.Html;
using AngleSharp.Parser.Html;
using Newtonsoft.Json.Linq;

namespace YoutubeExplode.Internal.Parsers
{
    internal partial class VideoEmbedPageParser
    {
        private readonly IHtmlDocument _root;

        public VideoEmbedPageParser(IHtmlDocument root)
        {
            _root = root;
        }

        public ConfigParser GetConfig()
        {
            var configRaw = Regex.Match(_root.Source.Text,
                    @"yt\.setConfig\({'PLAYER_CONFIG': (?<Json>\{[^\{\}]*(((?<Open>\{)[^\{\}]*)+((?<Close-Open>\})[^\{\}]*)+)*(?(Open)(?!))\})")
                .Groups["Json"].Value;
            var configJson = JToken.Parse(configRaw);

            return new ConfigParser(configJson);
        }
    }

    internal partial class VideoEmbedPageParser
    {
        public class ConfigParser
        {
            private readonly JToken _root;

            public ConfigParser(JToken root)
            {
                _root = root;
            }

            public string ParseChannelId()
            {
                var channelPath = _root.SelectToken("args.channel_path").Value<string>();
                var channelId = channelPath.SubstringAfter("channel/");

                return channelId;
            }

            public string ParseChannelTitle() => _root.SelectToken("args.expanded_title").Value<string>();

            public string ParseChannelLogoUrl() => _root.SelectToken("args.profile_picture").Value<string>();
        }
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