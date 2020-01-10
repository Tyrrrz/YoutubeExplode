using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.JsonApiModels
{
    internal class Info
    {
        [JsonProperty("st")]
        public long St { get; set; }
    }
}