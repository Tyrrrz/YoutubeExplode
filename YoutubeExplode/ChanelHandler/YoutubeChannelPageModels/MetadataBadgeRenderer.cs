using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class MetadataBadgeRenderer
    {
        [JsonProperty("icon")]
        public IconImageClass Icon { get; set; }

        [JsonProperty("style")]
        public Style Style { get; set; }

        [JsonProperty("tooltip")]
        public Tooltip Tooltip { get; set; }

        [JsonProperty("trackingParams")]
        public string TrackingParams { get; set; }
    }
}