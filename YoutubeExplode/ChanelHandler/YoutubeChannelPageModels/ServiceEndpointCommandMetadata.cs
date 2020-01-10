using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class ServiceEndpointCommandMetadata
    {
        [JsonProperty("webCommandMetadata")]
        public TentacledWebCommandMetadata WebCommandMetadata { get; set; }
    }
}