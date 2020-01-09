using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class CommandOpenPopupAction
    {
        [JsonProperty("popup")]
        public PurplePopup Popup { get; set; }

        [JsonProperty("popupType")]
        public string PopupType { get; set; }
    }
}