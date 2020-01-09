using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class SortSetting
    {
        [JsonProperty("sortFilterSubMenuRenderer")]
        public SortFilterSubMenuRenderer SortFilterSubMenuRenderer { get; set; }
    }
}