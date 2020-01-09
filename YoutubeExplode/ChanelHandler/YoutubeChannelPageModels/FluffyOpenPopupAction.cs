using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class FluffyOpenPopupAction
    {
        [JsonProperty("popup")]
        public TentacledPopup Popup { get; set; }

        [JsonProperty("popupType")]
        public string PopupType { get; set; }

        [JsonProperty("beReused")]
        public bool BeReused { get; set; }
    }
}