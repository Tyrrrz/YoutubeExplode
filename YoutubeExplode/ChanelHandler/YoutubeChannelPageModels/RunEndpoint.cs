using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class RunEndpoint
    {
        [JsonProperty("clickTrackingParams")]
        public string ClickTrackingParams { get; set; }

        [JsonProperty("commandMetadata")]
        public EndpointCommandMetadata CommandMetadata { get; set; }

        [JsonProperty("browseEndpoint")]
        public FluffyBrowseEndpoint BrowseEndpoint { get; set; }
    }
}