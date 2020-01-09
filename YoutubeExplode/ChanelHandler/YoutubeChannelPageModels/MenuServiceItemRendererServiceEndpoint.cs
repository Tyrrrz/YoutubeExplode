using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class MenuServiceItemRendererServiceEndpoint
    {
        [JsonProperty("clickTrackingParams")]
        public string ClickTrackingParams { get; set; }

        [JsonProperty("commandMetadata")]
        public ServiceEndpointCommandMetadata CommandMetadata { get; set; }

        [JsonProperty("modifyChannelNotificationPreferenceEndpoint")]
        public Endpoint ModifyChannelNotificationPreferenceEndpoint { get; set; }
    }
}