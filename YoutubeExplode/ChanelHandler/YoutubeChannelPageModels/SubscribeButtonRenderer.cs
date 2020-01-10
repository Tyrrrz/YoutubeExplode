using System.Collections.Generic;
using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class SubscribeButtonRenderer
    {
        [JsonProperty("buttonText")]
        public SubscriberCountText ButtonText { get; set; }

        [JsonProperty("subscriberCountText")]
        public ShortSubscriberCountText SubscriberCountText { get; set; }

        [JsonProperty("subscribed")]
        public bool Subscribed { get; set; }

        [JsonProperty("enabled")]
        public bool Enabled { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("channelId")]
        public string ChannelId { get; set; }

        [JsonProperty("showPreferences")]
        public bool ShowPreferences { get; set; }

        [JsonProperty("subscriberCountWithUnsubscribeText")]
        public ShortSubscriberCountText SubscriberCountWithUnsubscribeText { get; set; }

        [JsonProperty("subscribedButtonText")]
        public SubscriberCountText SubscribedButtonText { get; set; }

        [JsonProperty("unsubscribedButtonText")]
        public SubscriberCountText UnsubscribedButtonText { get; set; }

        [JsonProperty("trackingParams")]
        public string TrackingParams { get; set; }

        [JsonProperty("unsubscribeButtonText")]
        public SubscriberCountText UnsubscribeButtonText { get; set; }

        [JsonProperty("longSubscriberCountText")]
        public SubscriberCountText LongSubscriberCountText { get; set; }

        [JsonProperty("shortSubscriberCountText")]
        public ShortSubscriberCountText ShortSubscriberCountText { get; set; }

        [JsonProperty("subscribeAccessibility")]
        public AccessibilityData SubscribeAccessibility { get; set; }

        [JsonProperty("unsubscribeAccessibility")]
        public AccessibilityData UnsubscribeAccessibility { get; set; }

        [JsonProperty("notificationPreferenceButton")]
        public NotificationPreferenceButton NotificationPreferenceButton { get; set; }

        [JsonProperty("subscribedEntityKey")]
        public string SubscribedEntityKey { get; set; }

        [JsonProperty("onSubscribeEndpoints")]
        public List<OnSubscribeEndpoint> OnSubscribeEndpoints { get; set; }

        [JsonProperty("onUnsubscribeEndpoints")]
        public List<OnUnsubscribeEndpoint> OnUnsubscribeEndpoints { get; set; }
    }
}