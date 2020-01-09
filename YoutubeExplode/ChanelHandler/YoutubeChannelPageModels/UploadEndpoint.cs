using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class UploadEndpoint
    {
        [JsonProperty("hack")]
        public bool Hack { get; set; }
    }
}