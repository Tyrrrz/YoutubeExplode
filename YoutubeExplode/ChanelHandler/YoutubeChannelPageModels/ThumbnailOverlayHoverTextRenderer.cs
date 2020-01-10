using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class ThumbnailOverlayHoverTextRenderer
    {
        [JsonProperty("text")]
        public SubscriberCountText Text { get; set; }

        [JsonProperty("icon")]
        public IconImageClass Icon { get; set; }
    }
}