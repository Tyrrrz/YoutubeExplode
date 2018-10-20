using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace YoutubeExplode.Internal.Parsers
{
    internal partial class VideoEmbedPageParser
    {
        private readonly JToken _root;

        public VideoEmbedPageParser(JToken root)
        {
            _root = root;
        }

        public string ParsePlayerSourceUrl()
        {
            var relativeUrl = _root["assets"]["js"].Value<string>();

            if (relativeUrl.IsNotBlank())
                relativeUrl = "https://www.youtube.com" + relativeUrl;

            return relativeUrl;
        }

        public string ParseSts() => _root["sts"].Value<string>();

        public string ParseChannelId()
        {
            var channelPath = _root["args"]["channel_path"].Value<string>();
            var channelId = channelPath.SubstringAfter("channel/");

            return channelId;
        }

        public string ParseChannelTitle() => _root["args"]["expanded_title"].Value<string>();

        public string ParseChannelLogoUrl() => _root["args"]["profile_picture"].Value<string>();
    }

    internal partial class VideoEmbedPageParser
    {
        public static VideoEmbedPageParser Initialize(string raw)
        {
            // Extract the config part
            var config = Regex.Match(raw,
                    @"yt\.setConfig\({'PLAYER_CONFIG': (?<Json>\{[^\{\}]*(((?<Open>\{)[^\{\}]*)+((?<Close-Open>\})[^\{\}]*)+)*(?(Open)(?!))\})")
                .Groups["Json"].Value;

            var root = JToken.Parse(config);

            return new VideoEmbedPageParser(root);
        }
    }
}