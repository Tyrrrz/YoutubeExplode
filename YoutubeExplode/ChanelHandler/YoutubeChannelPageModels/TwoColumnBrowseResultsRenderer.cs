using System.Collections.Generic;
using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class TwoColumnBrowseResultsRenderer
    {
        [JsonProperty("tabs")]
        public List<Tab> Tabs { get; set; }
    }
}