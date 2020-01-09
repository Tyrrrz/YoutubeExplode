using System.Collections.Generic;
using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class MenuRequestSignalServiceEndpoint
    {
        [JsonProperty("signal")]
        public string Signal { get; set; }

        [JsonProperty("actions")]
        public List<TentacledAction> Actions { get; set; }
    }
}