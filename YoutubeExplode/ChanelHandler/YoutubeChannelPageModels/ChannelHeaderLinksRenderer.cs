using System.Collections.Generic;
using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class ChannelHeaderLinksRenderer
    {
        [JsonProperty("primaryLinks")]
        public List<AryLink> PrimaryLinks { get; set; }

        [JsonProperty("secondaryLinks")]
        public List<AryLink> SecondaryLinks { get; set; }

        [JsonProperty("hack")]
        public bool Hack { get; set; }
    }
}