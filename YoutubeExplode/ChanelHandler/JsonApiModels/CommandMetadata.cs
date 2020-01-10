using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.JsonApiModels
{
    internal class CommandMetadata
    {
        [JsonProperty("webCommandMetadata")]
        public WebCommandMetadata WebCommandMetadata { get; set; }
    }
}