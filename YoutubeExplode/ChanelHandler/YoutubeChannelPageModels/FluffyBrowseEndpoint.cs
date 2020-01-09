using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class FluffyBrowseEndpoint
    {
        [JsonProperty("browseId")]
        public string BrowseId { get; set; }
    }
}