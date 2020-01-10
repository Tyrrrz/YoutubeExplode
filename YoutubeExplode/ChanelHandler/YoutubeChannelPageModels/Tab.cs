using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class Tab
    {
        [JsonProperty("tabRenderer", NullValueHandling = NullValueHandling.Ignore)]
        public TabRenderer TabRenderer { get; set; }

        [JsonProperty("expandableTabRenderer", NullValueHandling = NullValueHandling.Ignore)]
        public ExpandableTabRenderer ExpandableTabRenderer { get; set; }
    }
}