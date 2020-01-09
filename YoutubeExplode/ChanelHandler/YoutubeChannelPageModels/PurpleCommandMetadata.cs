using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class PurpleCommandMetadata
    {
        [JsonProperty("webCommandMetadata")]
        public FluffyWebCommandMetadata WebCommandMetadata { get; set; }
    }
}