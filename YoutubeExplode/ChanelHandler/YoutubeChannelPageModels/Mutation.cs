using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class Mutation
    {
        [JsonProperty("entityKey")]
        public string EntityKey { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("payload")]
        public Payload Payload { get; set; }
    }
}