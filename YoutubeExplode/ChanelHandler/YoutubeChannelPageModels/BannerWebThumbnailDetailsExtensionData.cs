using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class BannerWebThumbnailDetailsExtensionData
    {
        [JsonProperty("isPreloaded")]
        public bool IsPreloaded { get; set; }
    }
}