using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class EndpointCommandMetadata
    {
        [JsonProperty("webCommandMetadata")]
        public PurpleWebCommandMetadata WebCommandMetadata { get; set; }
    }
}