using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class Metadata
    {
        [JsonProperty("channelMetadataRenderer")]
        public ChannelMetadataRenderer ChannelMetadataRenderer { get; set; }
    }
}