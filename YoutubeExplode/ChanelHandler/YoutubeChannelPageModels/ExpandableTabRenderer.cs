using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class ExpandableTabRenderer
    {
        [JsonProperty("endpoint")]
        public PurpleEndpoint Endpoint { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("selected")]
        public bool Selected { get; set; }
    }
}