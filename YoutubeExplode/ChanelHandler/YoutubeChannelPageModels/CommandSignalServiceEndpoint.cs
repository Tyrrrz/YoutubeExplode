using System.Collections.Generic;
using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class CommandSignalServiceEndpoint
    {
        [JsonProperty("signal")]
        public string Signal { get; set; }

        [JsonProperty("actions")]
        public List<FluffyAction> Actions { get; set; }
    }
}