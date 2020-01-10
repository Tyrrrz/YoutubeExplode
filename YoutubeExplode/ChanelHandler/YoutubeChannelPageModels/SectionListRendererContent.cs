using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class SectionListRendererContent
    {
        [JsonProperty("itemSectionRenderer")]
        public ItemSectionRenderer ItemSectionRenderer { get; set; }
    }
}