using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class FusionSearchboxRenderer
    {
        [JsonProperty("icon")]
        public IconImageClass Icon { get; set; }

        [JsonProperty("placeholderText")]
        public SubscriberCountText PlaceholderText { get; set; }

        [JsonProperty("config")]
        public Config Config { get; set; }

        [JsonProperty("trackingParams")]
        public string TrackingParams { get; set; }

        [JsonProperty("searchEndpoint")]
        public FusionSearchboxRendererSearchEndpoint SearchEndpoint { get; set; }
    }
}