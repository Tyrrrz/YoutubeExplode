using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class SubscriptionNotificationToggleButtonRendererCommand
    {
        [JsonProperty("commandExecutorCommand")]
        public CommandExecutorCommand CommandExecutorCommand { get; set; }
    }
}