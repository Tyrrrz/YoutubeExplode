using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class FluffyWebCommandMetadata
    {
        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("rootVe")]
        public long RootVe { get; set; }
    }
}