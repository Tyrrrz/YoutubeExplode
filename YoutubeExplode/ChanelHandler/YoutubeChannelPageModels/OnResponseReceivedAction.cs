using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class OnResponseReceivedAction
    {
        [JsonProperty("resetChannelUnreadCountCommand")]
        public ResetChannelUnreadCountCommand ResetChannelUnreadCountCommand { get; set; }
    }
}