using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class TentacledPopup
    {
        [JsonProperty("multiPageMenuRenderer")]
        public PopupMultiPageMenuRenderer MultiPageMenuRenderer { get; set; }
    }
}