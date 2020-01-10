using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class NextContinuationData
    {
        [JsonProperty("continuation")]
        public string Continuation { get; set; }

        [JsonProperty("clickTrackingParams")]
        public string ClickTrackingParams { get; set; }
    }
}