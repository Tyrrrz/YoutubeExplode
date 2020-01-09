using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class SubscribeButton
    {
        [JsonProperty("subscribeButtonRenderer")]
        public SubscribeButtonRenderer SubscribeButtonRenderer { get; set; }
    }
}