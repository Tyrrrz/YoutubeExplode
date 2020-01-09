using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class SubscriptionStateEntity
    {
        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("subscribed")]
        public bool Subscribed { get; set; }
    }
}