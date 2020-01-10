using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class StateElement
    {
        [JsonProperty("stateId")]
        public long StateId { get; set; }

        [JsonProperty("nextStateId")]
        public long NextStateId { get; set; }

        [JsonProperty("state")]
        public StateState State { get; set; }
    }
}