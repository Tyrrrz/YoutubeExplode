using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class LinkAlternate
    {
        [JsonProperty("hrefUrl")]
        public string HrefUrl { get; set; }
    }
}