using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class PopupMultiPageMenuRenderer
    {
        [JsonProperty("trackingParams")]
        public string TrackingParams { get; set; }

        [JsonProperty("style")]
        public string Style { get; set; }

        [JsonProperty("showLoadingSpinner")]
        public bool ShowLoadingSpinner { get; set; }
    }
}