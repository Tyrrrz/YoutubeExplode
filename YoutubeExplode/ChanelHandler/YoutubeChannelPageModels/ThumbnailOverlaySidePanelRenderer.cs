using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class ThumbnailOverlaySidePanelRenderer
    {
        [JsonProperty("text")]
        public ShortSubscriberCountText Text { get; set; }

        [JsonProperty("icon")]
        public IconImageClass Icon { get; set; }
    }
}