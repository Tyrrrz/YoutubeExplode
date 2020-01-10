using Newtonsoft.Json;
using YoutubeExplode.ChanelHandler.ChannelPageModels;

namespace YoutubeExplode.ChanelHandler.JsonApiModels
{
    internal class Response
    {
        [JsonProperty("responseContext")]
        public ResponseContext ResponseContext { get; set; }

        [JsonProperty("trackingParams")]
        public string TrackingParams { get; set; }

        [JsonProperty("metadata")]
        public Metadata Metadata { get; set; }

        [JsonProperty("continuationContents")]
        public ContinuationContents ContinuationContents { get; set; }

        [JsonProperty("microformat")]
        public Microformat Microformat { get; set; }
    }
}