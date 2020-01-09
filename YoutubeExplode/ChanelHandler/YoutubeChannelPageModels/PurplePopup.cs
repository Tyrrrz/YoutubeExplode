using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class PurplePopup
    {
        [JsonProperty("menuPopupRenderer")]
        public MenuPopupRenderer MenuPopupRenderer { get; set; }
    }
}