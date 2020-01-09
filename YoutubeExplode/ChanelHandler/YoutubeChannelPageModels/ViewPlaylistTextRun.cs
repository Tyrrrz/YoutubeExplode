using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class ViewPlaylistTextRun
    {
        [JsonProperty("text")]
        public RunTextEnum Text { get; set; }

        [JsonProperty("navigationEndpoint")]
        public RunEndpoint NavigationEndpoint { get; set; }
    }
}