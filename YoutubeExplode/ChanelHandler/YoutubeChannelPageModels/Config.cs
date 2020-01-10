using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class Config
    {
        [JsonProperty("webSearchboxConfig")]
        public WebSearchboxConfig WebSearchboxConfig { get; set; }
    }
}