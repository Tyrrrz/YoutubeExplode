using System.Collections.Generic;
using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class MenuPopupRenderer
    {
        [JsonProperty("items")]
        public List<MenuPopupRendererItem> Items { get; set; }
    }
}