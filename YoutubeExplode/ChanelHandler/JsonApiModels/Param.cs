using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.JsonApiModels
{
    internal class Param
    {
        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }
    }
}