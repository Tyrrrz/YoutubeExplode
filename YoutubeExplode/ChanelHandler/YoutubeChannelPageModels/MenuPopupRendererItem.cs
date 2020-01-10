using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class MenuPopupRendererItem
    {
        [JsonProperty("menuServiceItemRenderer")]
        public MenuServiceItemRenderer MenuServiceItemRenderer { get; set; }
    }
}