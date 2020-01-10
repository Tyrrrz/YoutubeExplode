using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class SponsorButton
    {
        [JsonProperty("buttonRenderer")]
        public SponsorButtonButtonRenderer ButtonRenderer { get; set; }
    }
}