using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class PurpleWebCommandMetadata
    {
        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("webPageType")]
        public WebPageType WebPageType { get; set; }

        [JsonProperty("rootVe")]
        public long RootVe { get; set; }
    }
}