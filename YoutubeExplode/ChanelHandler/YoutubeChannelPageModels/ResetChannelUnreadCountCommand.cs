using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class ResetChannelUnreadCountCommand
    {
        [JsonProperty("channelId")]
        public string ChannelId { get; set; }
    }
}