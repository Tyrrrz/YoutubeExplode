using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class TopbarMenuButtonRenderer
    {
        [JsonProperty("icon", NullValueHandling = NullValueHandling.Ignore)]
        public IconImageClass Icon { get; set; }

        [JsonProperty("menuRenderer", NullValueHandling = NullValueHandling.Ignore)]
        public MenuRenderer MenuRenderer { get; set; }

        [JsonProperty("trackingParams")]
        public string TrackingParams { get; set; }

        [JsonProperty("accessibility")]
        public AccessibilityData Accessibility { get; set; }

        [JsonProperty("tooltip")]
        public string Tooltip { get; set; }

        [JsonProperty("style", NullValueHandling = NullValueHandling.Ignore)]
        public string Style { get; set; }

        [JsonProperty("avatar", NullValueHandling = NullValueHandling.Ignore)]
        public TopbarMenuButtonRendererAvatar Avatar { get; set; }

        [JsonProperty("menuRequest", NullValueHandling = NullValueHandling.Ignore)]
        public MenuRequest MenuRequest { get; set; }
    }
}