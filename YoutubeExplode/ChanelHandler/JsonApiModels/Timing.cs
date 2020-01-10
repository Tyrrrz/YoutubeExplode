using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.JsonApiModels
{
    internal class Timing
    {
        [JsonProperty("info")]
        public Info Info { get; set; }
    }
}