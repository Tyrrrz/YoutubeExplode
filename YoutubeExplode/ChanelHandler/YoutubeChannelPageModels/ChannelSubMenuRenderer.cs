using System.Collections.Generic;
using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class ChannelSubMenuRenderer
    {
        [JsonProperty("contentTypeSubMenuItems")]
        public List<ExpandableTabRenderer> ContentTypeSubMenuItems { get; set; }

        [JsonProperty("flowSubMenuItems")]
        public List<ExpandableTabRenderer> FlowSubMenuItems { get; set; }

        [JsonProperty("sortSetting")]
        public SortSetting SortSetting { get; set; }
    }
}