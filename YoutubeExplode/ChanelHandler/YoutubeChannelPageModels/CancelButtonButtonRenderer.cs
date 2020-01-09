using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class CancelButtonButtonRenderer
    {
        [JsonProperty("style")]
        public string Style { get; set; }

        [JsonProperty("size")]
        public string Size { get; set; }

        [JsonProperty("text")]
        public SubscriberCountText Text { get; set; }

        [JsonProperty("trackingParams")]
        public string TrackingParams { get; set; }

        [JsonProperty("serviceEndpoint", NullValueHandling = NullValueHandling.Ignore)]
        public FluffyServiceEndpoint ServiceEndpoint { get; set; }
    }
}