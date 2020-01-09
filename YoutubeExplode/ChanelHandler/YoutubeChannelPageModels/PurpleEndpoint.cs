using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class PurpleEndpoint
    {
        [JsonProperty("clickTrackingParams")]
        public string ClickTrackingParams { get; set; }

        [JsonProperty("commandMetadata")]
        public EndpointCommandMetadata CommandMetadata { get; set; }

        [JsonProperty("browseEndpoint")]
        public PurpleBrowseEndpoint BrowseEndpoint { get; set; }
    }
}