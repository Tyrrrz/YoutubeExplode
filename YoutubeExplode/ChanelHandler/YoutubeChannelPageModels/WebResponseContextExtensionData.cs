using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class WebResponseContextExtensionData
    {
        [JsonProperty("webResponseContextPreloadData")]
        public WebResponseContextPreloadData WebResponseContextPreloadData { get; set; }

        [JsonProperty("ytConfigData")]
        public YtConfigData YtConfigData { get; set; }
    }
}