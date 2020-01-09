using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.JsonApiModels
{
    internal class YtConfigData
    {
        [JsonProperty("csn")]
        public string Csn { get; set; }

        [JsonProperty("visitorData")]
        public string VisitorData { get; set; }
    }
}