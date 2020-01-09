using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class NotificationPreferenceButton
    {
        [JsonProperty("subscriptionNotificationToggleButtonRenderer")]
        public SubscriptionNotificationToggleButtonRenderer SubscriptionNotificationToggleButtonRenderer { get; set; }
    }
}