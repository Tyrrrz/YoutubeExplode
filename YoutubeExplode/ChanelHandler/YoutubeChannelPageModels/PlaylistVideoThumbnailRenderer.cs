using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class PlaylistVideoThumbnailRenderer
    {
        [JsonProperty("thumbnail")]
        public BannerClass Thumbnail { get; set; }
    }
}