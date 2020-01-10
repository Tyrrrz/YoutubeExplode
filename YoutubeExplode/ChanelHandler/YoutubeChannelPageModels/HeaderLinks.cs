using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class HeaderLinks
    {
        [JsonProperty("channelHeaderLinksRenderer")]
        public ChannelHeaderLinksRenderer ChannelHeaderLinksRenderer { get; set; }
    }
}