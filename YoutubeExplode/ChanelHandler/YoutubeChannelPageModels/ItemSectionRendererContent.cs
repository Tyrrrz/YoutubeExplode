using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class ItemSectionRendererContent
    {
        [JsonProperty("gridRenderer")]
        public GridRenderer GridRenderer { get; set; }
    }
}