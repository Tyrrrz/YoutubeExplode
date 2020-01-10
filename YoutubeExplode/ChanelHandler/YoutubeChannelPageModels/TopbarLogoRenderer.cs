using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class TopbarLogoRenderer
    {
        [JsonProperty("iconImage")]
        public IconImageClass IconImage { get; set; }

        [JsonProperty("tooltipText")]
        public SubscriberCountText TooltipText { get; set; }

        [JsonProperty("endpoint")]
        public RunEndpoint Endpoint { get; set; }

        [JsonProperty("trackingParams")]
        public string TrackingParams { get; set; }
    }
}