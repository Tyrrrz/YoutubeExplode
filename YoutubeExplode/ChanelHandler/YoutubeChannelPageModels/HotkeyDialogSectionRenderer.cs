using System.Collections.Generic;
using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class HotkeyDialogSectionRenderer
    {
        [JsonProperty("title")]
        public SubscriberCountText Title { get; set; }

        [JsonProperty("options")]
        public List<Option> Options { get; set; }
    }
}