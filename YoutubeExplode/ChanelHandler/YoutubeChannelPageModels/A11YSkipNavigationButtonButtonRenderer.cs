using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class A11YSkipNavigationButtonButtonRenderer
    {
        [JsonProperty("style")]
        public string Style { get; set; }

        [JsonProperty("size")]
        public string Size { get; set; }

        [JsonProperty("isDisabled")]
        public bool IsDisabled { get; set; }

        [JsonProperty("text")]
        public SubscriberCountText Text { get; set; }

        [JsonProperty("trackingParams")]
        public string TrackingParams { get; set; }

        [JsonProperty("command")]
        public ButtonRendererCommand Command { get; set; }
    }
}