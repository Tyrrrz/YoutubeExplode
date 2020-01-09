using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class StateButtonRenderer
    {
        [JsonProperty("style")]
        public string Style { get; set; }

        [JsonProperty("size")]
        public string Size { get; set; }

        [JsonProperty("isDisabled")]
        public bool IsDisabled { get; set; }

        [JsonProperty("icon")]
        public IconImageClass Icon { get; set; }

        [JsonProperty("accessibility")]
        public Accessibility Accessibility { get; set; }

        [JsonProperty("trackingParams")]
        public string TrackingParams { get; set; }

        [JsonProperty("accessibilityData")]
        public AccessibilityData AccessibilityData { get; set; }
    }
}