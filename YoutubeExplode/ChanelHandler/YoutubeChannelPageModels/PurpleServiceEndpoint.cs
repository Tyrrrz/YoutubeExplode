using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class PurpleServiceEndpoint
    {
        [JsonProperty("clickTrackingParams")]
        public string ClickTrackingParams { get; set; }

        [JsonProperty("commandMetadata")]
        public ServiceEndpointCommandMetadata CommandMetadata { get; set; }

        [JsonProperty("ypcGetOffersEndpoint")]
        public Endpoint YpcGetOffersEndpoint { get; set; }
    }
}