using System.Collections.Generic;
using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class DesktopTopbarRenderer
    {
        [JsonProperty("logo")]
        public Logo Logo { get; set; }

        [JsonProperty("searchbox")]
        public Searchbox Searchbox { get; set; }

        [JsonProperty("trackingParams")]
        public string TrackingParams { get; set; }

        [JsonProperty("countryCode")]
        public string CountryCode { get; set; }

        [JsonProperty("topbarButtons")]
        public List<TopbarButton> TopbarButtons { get; set; }

        [JsonProperty("hotkeyDialog")]
        public HotkeyDialog HotkeyDialog { get; set; }

        [JsonProperty("backButton")]
        public BackButtonClass BackButton { get; set; }

        [JsonProperty("forwardButton")]
        public BackButtonClass ForwardButton { get; set; }

        [JsonProperty("a11ySkipNavigationButton")]
        public A11YSkipNavigationButton A11YSkipNavigationButton { get; set; }
    }
}