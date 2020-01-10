using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.JsonApiModels
{
    internal class WebCommandMetadata
    {
        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("webPageType")]
        public string WebPageType { get; set; }

        [JsonProperty("rootVe")]
        public long RootVe { get; set; }
    }
}