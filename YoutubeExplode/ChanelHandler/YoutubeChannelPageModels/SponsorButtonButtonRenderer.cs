using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class SponsorButtonButtonRenderer
    {
        [JsonProperty("style")]
        public string Style { get; set; }

        [JsonProperty("size")]
        public string Size { get; set; }

        [JsonProperty("text")]
        public SubscriberCountText Text { get; set; }

        [JsonProperty("serviceEndpoint")]
        public PurpleServiceEndpoint ServiceEndpoint { get; set; }

        [JsonProperty("trackingParams")]
        public string TrackingParams { get; set; }

        [JsonProperty("hint")]
        public Hint Hint { get; set; }

        [JsonProperty("accessibilityData")]
        public AccessibilityData AccessibilityData { get; set; }
    }
}