using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class GridPlaylistRendererNavigationEndpoint
    {
        [JsonProperty("clickTrackingParams")]
        public string ClickTrackingParams { get; set; }

        [JsonProperty("commandMetadata")]
        public EndpointCommandMetadata CommandMetadata { get; set; }

        [JsonProperty("watchEndpoint")]
        public WatchEndpoint WatchEndpoint { get; set; }
    }
}