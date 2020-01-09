using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class IconImageClass
    {
        [JsonProperty("iconType")]
        public string IconType { get; set; }
    }
}