using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class CommandElement
    {
        [JsonProperty("openPopupAction")]
        public CommandOpenPopupAction OpenPopupAction { get; set; }
    }
}