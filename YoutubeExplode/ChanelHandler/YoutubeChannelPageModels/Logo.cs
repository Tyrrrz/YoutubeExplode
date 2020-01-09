using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class Logo
    {
        [JsonProperty("topbarLogoRenderer")]
        public TopbarLogoRenderer TopbarLogoRenderer { get; set; }
    }
}