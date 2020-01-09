using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class DismissButtonButtonRenderer
    {
        [JsonProperty("style")]
        public string Style { get; set; }

        [JsonProperty("size")]
        public string Size { get; set; }

        [JsonProperty("isDisabled")]
        public bool IsDisabled { get; set; }

        [JsonProperty("text")]
        public ShortSubscriberCountText Text { get; set; }

        [JsonProperty("trackingParams")]
        public string TrackingParams { get; set; }
    }
}