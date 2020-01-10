using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class TopbarButton
    {
        [JsonProperty("topbarMenuButtonRenderer", NullValueHandling = NullValueHandling.Ignore)]
        public TopbarMenuButtonRenderer TopbarMenuButtonRenderer { get; set; }

        [JsonProperty("notificationTopbarButtonRenderer", NullValueHandling = NullValueHandling.Ignore)]
        public NotificationTopbarButtonRenderer NotificationTopbarButtonRenderer { get; set; }
    }
}