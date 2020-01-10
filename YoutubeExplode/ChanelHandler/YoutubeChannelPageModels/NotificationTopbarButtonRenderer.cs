using System.Collections.Generic;
using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class NotificationTopbarButtonRenderer
    {
        [JsonProperty("icon")]
        public IconImageClass Icon { get; set; }

        [JsonProperty("menuRequest")]
        public MenuRequest MenuRequest { get; set; }

        [JsonProperty("style")]
        public string Style { get; set; }

        [JsonProperty("trackingParams")]
        public string TrackingParams { get; set; }

        [JsonProperty("accessibility")]
        public AccessibilityData Accessibility { get; set; }

        [JsonProperty("tooltip")]
        public string Tooltip { get; set; }

        [JsonProperty("updateUnseenCountEndpoint")]
        public UpdateUnseenCountEndpoint UpdateUnseenCountEndpoint { get; set; }

        [JsonProperty("notificationCount")]
        public long NotificationCount { get; set; }

        [JsonProperty("handlerDatas")]
        public List<string> HandlerDatas { get; set; }
    }
}