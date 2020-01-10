using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class TitleRun
    {
        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("navigationEndpoint")]
        public GridPlaylistRendererNavigationEndpoint NavigationEndpoint { get; set; }
    }
}