using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class PrimaryLinkNavigationEndpoint
    {
        [JsonProperty("clickTrackingParams")]
        public string ClickTrackingParams { get; set; }

        [JsonProperty("commandMetadata")]
        public PurpleCommandMetadata CommandMetadata { get; set; }

        [JsonProperty("urlEndpoint")]
        public PurpleUrlEndpoint UrlEndpoint { get; set; }
    }
}