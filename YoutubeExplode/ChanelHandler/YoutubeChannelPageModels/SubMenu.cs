using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class SubMenu
    {
        [JsonProperty("channelSubMenuRenderer")]
        public ChannelSubMenuRenderer ChannelSubMenuRenderer { get; set; }
    }
}