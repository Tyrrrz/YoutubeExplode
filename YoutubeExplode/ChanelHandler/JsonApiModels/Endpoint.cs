using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.JsonApiModels
{
    internal class Endpoint
    {
        [JsonProperty("commandMetadata")]
        public CommandMetadata CommandMetadata { get; set; }

        [JsonProperty("urlEndpoint")]
        public UrlEndpoint UrlEndpoint { get; set; }
    }
}