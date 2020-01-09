using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class StateState
    {
        [JsonProperty("buttonRenderer")]
        public StateButtonRenderer ButtonRenderer { get; set; }
    }
}