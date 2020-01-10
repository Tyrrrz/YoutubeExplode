using System.Collections.Generic;
using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class YoutubeSideData
    {
        [JsonProperty("responseContext")]
        public ResponseContext ResponseContext { get; set; }

        [JsonProperty("contents")]
        public Contents Contents { get; set; }

        [JsonProperty("header")]
        public Header Header { get; set; }

        [JsonProperty("metadata")]
        public Metadata Metadata { get; set; }

        [JsonProperty("trackingParams")]
        public string TrackingParams { get; set; }

        [JsonProperty("topbar")]
        public Topbar Topbar { get; set; }

        [JsonProperty("microformat")]
        public Microformat Microformat { get; set; }

        [JsonProperty("onResponseReceivedActions")]
        public List<OnResponseReceivedAction> OnResponseReceivedActions { get; set; }

        [JsonProperty("frameworkUpdates")]
        public FrameworkUpdates FrameworkUpdates { get; set; }
    }
}