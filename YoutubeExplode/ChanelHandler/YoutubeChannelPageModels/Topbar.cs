using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class Topbar
    {
        [JsonProperty("desktopTopbarRenderer")]
        public DesktopTopbarRenderer DesktopTopbarRenderer { get; set; }
    }
}