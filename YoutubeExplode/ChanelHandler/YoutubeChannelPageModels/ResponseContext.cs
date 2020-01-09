using System.Collections.Generic;
using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class ResponseContext
    {
        [JsonProperty("serviceTrackingParams")]
        public List<ServiceTrackingParam> ServiceTrackingParams { get; set; }

        [JsonProperty("maxAgeSeconds")]
        public long MaxAgeSeconds { get; set; }

        [JsonProperty("webResponseContextExtensionData")]
        public WebResponseContextExtensionData WebResponseContextExtensionData { get; set; }
    }
}