using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class CancelButtonClass
    {
        [JsonProperty("buttonRenderer")]
        public CancelButtonButtonRenderer ButtonRenderer { get; set; }
    }
}