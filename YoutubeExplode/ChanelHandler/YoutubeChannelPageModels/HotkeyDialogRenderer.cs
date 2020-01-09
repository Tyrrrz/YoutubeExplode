using System.Collections.Generic;
using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class HotkeyDialogRenderer
    {
        [JsonProperty("title")]
        public SubscriberCountText Title { get; set; }

        [JsonProperty("sections")]
        public List<HotkeyDialogRendererSection> Sections { get; set; }

        [JsonProperty("dismissButton")]
        public DismissButton DismissButton { get; set; }

        [JsonProperty("trackingParams")]
        public string TrackingParams { get; set; }
    }
}