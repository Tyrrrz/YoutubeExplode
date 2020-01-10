using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class MenuServiceItemRenderer
    {
        [JsonProperty("text")]
        public SubscriberCountText Text { get; set; }

        [JsonProperty("icon")]
        public IconImageClass Icon { get; set; }

        [JsonProperty("serviceEndpoint")]
        public MenuServiceItemRendererServiceEndpoint ServiceEndpoint { get; set; }

        [JsonProperty("trackingParams")]
        public string TrackingParams { get; set; }

        [JsonProperty("isSelected")]
        public bool IsSelected { get; set; }
    }
}