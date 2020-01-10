using System.Collections.Generic;
using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class SortFilterSubMenuRenderer
    {
        [JsonProperty("subMenuItems")]
        public List<SubMenuItem> SubMenuItems { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("icon")]
        public IconImageClass Icon { get; set; }

        [JsonProperty("accessibility")]
        public AccessibilityData Accessibility { get; set; }

        [JsonProperty("trackingParams")]
        public string TrackingParams { get; set; }
    }
}