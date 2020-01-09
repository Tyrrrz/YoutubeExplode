using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.JsonApiModels
{
    internal class UrlEndpoint
    {
        [JsonProperty("url")]
        public string Url { get; set; }
    }
}