using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class Payload
    {
        [JsonProperty("subscriptionStateEntity")]
        public SubscriptionStateEntity SubscriptionStateEntity { get; set; }
    }
}