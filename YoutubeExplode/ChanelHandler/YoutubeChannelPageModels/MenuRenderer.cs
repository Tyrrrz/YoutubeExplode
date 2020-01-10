using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class MenuRenderer
    {
        [JsonProperty("multiPageMenuRenderer")]
        public MenuRendererMultiPageMenuRenderer MultiPageMenuRenderer { get; set; }
    }
}