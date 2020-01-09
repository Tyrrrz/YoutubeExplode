using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class PurpleWebThumbnailDetailsExtensionData
    {
        [JsonProperty("excludeFromVpl")]
        public bool ExcludeFromVpl { get; set; }
    }
}