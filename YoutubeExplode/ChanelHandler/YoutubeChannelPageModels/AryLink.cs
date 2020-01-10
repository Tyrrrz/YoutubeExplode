using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class AryLink
    {
        [JsonProperty("navigationEndpoint")]
        public PrimaryLinkNavigationEndpoint NavigationEndpoint { get; set; }

        [JsonProperty("icon")]
        public PrimaryLinkIcon Icon { get; set; }

        [JsonProperty("title")]
        public ShortSubscriberCountText Title { get; set; }
    }
}