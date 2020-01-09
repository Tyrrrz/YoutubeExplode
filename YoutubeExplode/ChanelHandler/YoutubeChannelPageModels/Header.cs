using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class Header
    {
        [JsonProperty("c4TabbedHeaderRenderer")]
        public C4TabbedHeaderRenderer C4TabbedHeaderRenderer { get; set; }
    }
}