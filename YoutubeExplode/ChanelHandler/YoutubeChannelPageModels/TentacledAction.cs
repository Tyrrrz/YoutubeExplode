using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class TentacledAction
    {
        [JsonProperty("openPopupAction")]
        public FluffyOpenPopupAction OpenPopupAction { get; set; }
    }
}