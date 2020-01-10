using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class PurpleAction
    {
        [JsonProperty("openPopupAction")]
        public PurpleOpenPopupAction OpenPopupAction { get; set; }
    }
}