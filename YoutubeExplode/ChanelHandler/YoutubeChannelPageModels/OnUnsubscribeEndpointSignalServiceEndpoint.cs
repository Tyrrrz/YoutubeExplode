using System.Collections.Generic;
using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class OnUnsubscribeEndpointSignalServiceEndpoint
    {
        [JsonProperty("signal")]
        public string Signal { get; set; }

        [JsonProperty("actions")]
        public List<PurpleAction> Actions { get; set; }
    }
}