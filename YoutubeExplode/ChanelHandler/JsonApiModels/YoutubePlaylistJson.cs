using Newtonsoft.Json;
using YoutubeExplode.ChanelHandler.ChannelPageModels;

namespace YoutubeExplode.ChanelHandler.JsonApiModels
{
    internal class YoutubePlaylistJson
    {
        [JsonProperty("response")]
        public YoutubeSideData Response { get; set; }

        [JsonProperty("timing")]
        public Timing Timing { get; set; }

        [JsonProperty("endpoint")]
        public ChannelPageModels.Endpoint Endpoint { get; set; }

        [JsonProperty("page")]
        public string Page { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("xsrf_token ")]
        public string XsrfToken { get; set; }

        [JsonProperty("csn")]
        public string Csn { get; set; }
    }
}