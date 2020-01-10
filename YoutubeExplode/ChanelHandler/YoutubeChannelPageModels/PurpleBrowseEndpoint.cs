using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class PurpleBrowseEndpoint
    {
        [JsonProperty("browseId")]
        public string BrowseId { get; set; }

        [JsonProperty("params")]
        public string Params { get; set; }

        [JsonProperty("canonicalBaseUrl")]
        public string CanonicalBaseUrl { get; set; }
    }
}