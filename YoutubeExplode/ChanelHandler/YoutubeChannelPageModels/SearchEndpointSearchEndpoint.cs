using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class SearchEndpointSearchEndpoint
    {
        [JsonProperty("query")]
        public string Query { get; set; }
    }
}