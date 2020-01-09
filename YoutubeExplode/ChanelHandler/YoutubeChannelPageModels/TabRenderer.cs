using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class TabRenderer
    {
        [JsonProperty("endpoint")]
        public PurpleEndpoint Endpoint { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("selected")]
        public bool Selected { get; set; }

        [JsonProperty("trackingParams")]
        public string TrackingParams { get; set; }

        [JsonProperty("content", NullValueHandling = NullValueHandling.Ignore)]
        public TabRendererContent Content { get; set; }
    }
}