using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class CompactLinkRendererNavigationEndpoint
    {
        [JsonProperty("clickTrackingParams")]
        public string ClickTrackingParams { get; set; }

        [JsonProperty("commandMetadata")]
        public PurpleCommandMetadata CommandMetadata { get; set; }

        [JsonProperty("uploadEndpoint", NullValueHandling = NullValueHandling.Ignore)]
        public UploadEndpoint UploadEndpoint { get; set; }

        [JsonProperty("signalNavigationEndpoint", NullValueHandling = NullValueHandling.Ignore)]
        public Signal SignalNavigationEndpoint { get; set; }

        [JsonProperty("urlEndpoint", NullValueHandling = NullValueHandling.Ignore)]
        public FluffyUrlEndpoint UrlEndpoint { get; set; }
    }
}