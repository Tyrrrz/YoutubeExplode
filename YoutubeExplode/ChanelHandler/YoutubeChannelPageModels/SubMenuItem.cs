using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class SubMenuItem
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("selected")]
        public bool Selected { get; set; }

        [JsonProperty("navigationEndpoint")]
        public PurpleEndpoint NavigationEndpoint { get; set; }
    }
}