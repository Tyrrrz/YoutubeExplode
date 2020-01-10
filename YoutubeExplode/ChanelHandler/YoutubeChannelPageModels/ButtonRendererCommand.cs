using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class ButtonRendererCommand
    {
        [JsonProperty("clickTrackingParams")]
        public string ClickTrackingParams { get; set; }

        [JsonProperty("commandMetadata")]
        public ServiceEndpointCommandMetadata CommandMetadata { get; set; }

        [JsonProperty("signalServiceEndpoint")]
        public CommandSignalServiceEndpoint SignalServiceEndpoint { get; set; }
    }
}