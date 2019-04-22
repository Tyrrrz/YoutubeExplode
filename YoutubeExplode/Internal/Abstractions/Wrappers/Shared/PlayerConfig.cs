using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace YoutubeExplode.Internal.Abstractions.Wrappers.Shared
{
    internal class PlayerConfig
    {
        private readonly JToken _root;

        public PlayerConfig(JToken root)
        {
            _root = root;
        }

        public string GetSts() => _root.SelectToken("sts").Value<string>();

        public string GetPlayerSourceUrl()
        {
            var relativeUrl = _root.SelectToken("assets.js").Value<string>();

            if (!relativeUrl.IsNullOrWhiteSpace())
                relativeUrl = "https://www.youtube.com" + relativeUrl;

            return relativeUrl;
        }

        public string GetChannelId()
        {
            var channelPath = _root.SelectToken("args.channel_path").Value<string>();
            var channelId = channelPath.SubstringAfter("channel/");

            return channelId;
        }

        public string GetChannelTitle() => _root.SelectToken("args.expanded_title").Value<string>();

        public string GetChannelLogoUrl() => _root.SelectToken("args.profile_picture").Value<string>();

        public IEnumerable<StreamInfoUrlEncoded> GetMuxedStreamInfos()
        {
            var streamInfosEncoded = _root.SelectToken("args.url_encoded_fmt_stream_map").Value<string>();

            if (streamInfosEncoded.IsNullOrWhiteSpace())
                return Enumerable.Empty<StreamInfoUrlEncoded>();

            return streamInfosEncoded.Split(",")
                .Select(UrlEx.SplitQuery)
                .Select(d => new StreamInfoUrlEncoded(d));
        }

        public IEnumerable<StreamInfoUrlEncoded> GetAdaptiveStreamInfos()
        {
            var streamInfosEncoded = _root.SelectToken("args.adaptive_fmts").Value<string>();

            if (streamInfosEncoded.IsNullOrWhiteSpace())
                return Enumerable.Empty<StreamInfoUrlEncoded>();

            return streamInfosEncoded.Split(",")
                .Select(UrlEx.SplitQuery)
                .Select(d => new StreamInfoUrlEncoded(d));
        }
    }
}