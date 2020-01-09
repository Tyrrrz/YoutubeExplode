using System.Collections.Generic;
using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class SubscriptionNotificationToggleButtonRenderer
    {
        [JsonProperty("states")]
        public List<StateElement> States { get; set; }

        [JsonProperty("currentStateId")]
        public long CurrentStateId { get; set; }

        [JsonProperty("trackingParams")]
        public string TrackingParams { get; set; }

        [JsonProperty("command")]
        public SubscriptionNotificationToggleButtonRendererCommand Command { get; set; }
    }
}