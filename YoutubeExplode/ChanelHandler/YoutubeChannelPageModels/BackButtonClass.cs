using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class BackButtonClass
    {
        [JsonProperty("buttonRenderer")]
        public BackButtonButtonRenderer ButtonRenderer { get; set; }
    }
}