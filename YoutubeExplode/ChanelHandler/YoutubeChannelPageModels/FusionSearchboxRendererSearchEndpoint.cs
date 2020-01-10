using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class FusionSearchboxRendererSearchEndpoint
    {
        [JsonProperty("clickTrackingParams")]
        public string ClickTrackingParams { get; set; }

        [JsonProperty("commandMetadata")]
        public EndpointCommandMetadata CommandMetadata { get; set; }

        [JsonProperty("searchEndpoint")]
        public SearchEndpointSearchEndpoint SearchEndpoint { get; set; }
    }
}