using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal partial class A11YSkipNavigationButton
    {
        [JsonProperty("buttonRenderer")]
        public A11YSkipNavigationButtonButtonRenderer ButtonRenderer { get; set; }
    }
}