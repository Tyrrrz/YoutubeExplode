using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class MenuRequest
    {
        [JsonProperty("clickTrackingParams")]
        public string ClickTrackingParams { get; set; }

        [JsonProperty("commandMetadata")]
        public ServiceEndpointCommandMetadata CommandMetadata { get; set; }

        [JsonProperty("signalServiceEndpoint")]
        public MenuRequestSignalServiceEndpoint SignalServiceEndpoint { get; set; }
    }
}