using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace YoutubeExplode.Internal.Parsers
{
    internal partial class VideoEmbedPage
    {
        private readonly JToken _root;

        public VideoEmbedPage(JToken root)
        {
            _root = root;
        }

        public string GetPlayerSourceUrl()
        {
            var relativeUrl = _root["assets"]["js"].Value<string>();

            if (relativeUrl.IsNotBlank())
                relativeUrl = "https://www.youtube.com" + relativeUrl;

            return relativeUrl;
        }

        public string GetSts() => _root["sts"].Value<string>();

        public string GetChannelId()
        {
            var channelPath = _root["args"]["channel_path"].Value<string>();
            var channelId = channelPath.SubstringAfter("channel/");

            return channelId;
        }

        public string GetChannelTitle() => _root["args"]["expanded_title"].Value<string>();

        public string GetChannelLogoUrl() => _root["args"]["profile_picture"].Value<string>();
    }

    internal partial class VideoEmbedPage
    {
        public static VideoEmbedPage Parse(string raw)
        {
            var part = Regex.Match(raw,
                    @"yt\.setConfig\({'PLAYER_CONFIG': (?<Json>\{[^\{\}]*(((?<Open>\{)[^\{\}]*)+((?<Close-Open>\})[^\{\}]*)+)*(?(Open)(?!))\})")
                .Groups["Json"].Value;

            var root = JToken.Parse(part);

            return new VideoEmbedPage(root);
        }
    }
}