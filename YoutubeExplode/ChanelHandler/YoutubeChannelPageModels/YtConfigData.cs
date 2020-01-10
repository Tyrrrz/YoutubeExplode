using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class YtConfigData
    {
        [JsonProperty("csn")]
        public string Csn { get; set; }

        [JsonProperty("visitorData")]
        public string VisitorData { get; set; }

        [JsonProperty("sessionIndex")]
        public long SessionIndex { get; set; }

        [JsonProperty("delegatedSessionId")]
        public string DelegatedSessionId { get; set; }

        [JsonProperty("rootVisualElementType")]
        public long RootVisualElementType { get; set; }
    }
}