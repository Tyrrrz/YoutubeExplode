using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class UpdateUnseenCountEndpoint
    {
        [JsonProperty("clickTrackingParams")]
        public string ClickTrackingParams { get; set; }

        [JsonProperty("commandMetadata")]
        public ServiceEndpointCommandMetadata CommandMetadata { get; set; }

        [JsonProperty("signalServiceEndpoint")]
        public Signal SignalServiceEndpoint { get; set; }
    }
}