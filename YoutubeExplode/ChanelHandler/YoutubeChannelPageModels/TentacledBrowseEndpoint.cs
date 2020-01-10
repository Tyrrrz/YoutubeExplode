using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class TentacledBrowseEndpoint
    {
        [JsonProperty("browseId")]
        public string BrowseId { get; set; }

        [JsonProperty("canonicalBaseUrl")]
        public string CanonicalBaseUrl { get; set; }
    }
}