using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class PurpleOpenPopupAction
    {
        [JsonProperty("popup")]
        public FluffyPopup Popup { get; set; }

        [JsonProperty("popupType")]
        public string PopupType { get; set; }
    }
}