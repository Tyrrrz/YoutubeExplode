using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class DismissButton
    {
        [JsonProperty("buttonRenderer")]
        public DismissButtonButtonRenderer ButtonRenderer { get; set; }
    }
}