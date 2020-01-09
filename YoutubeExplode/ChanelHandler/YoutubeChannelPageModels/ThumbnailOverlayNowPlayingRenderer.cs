using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class ThumbnailOverlayNowPlayingRenderer
    {
        [JsonProperty("text")]
        public SubscriberCountText Text { get; set; }
    }
}