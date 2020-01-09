using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class CompactLinkRenderer
    {
        [JsonProperty("icon")]
        public IconImageClass Icon { get; set; }

        [JsonProperty("title")]
        public SubscriberCountText Title { get; set; }

        [JsonProperty("navigationEndpoint")]
        public CompactLinkRendererNavigationEndpoint NavigationEndpoint { get; set; }

        [JsonProperty("trackingParams")]
        public string TrackingParams { get; set; }

        [JsonProperty("style", NullValueHandling = NullValueHandling.Ignore)]
        public string Style { get; set; }
    }
}