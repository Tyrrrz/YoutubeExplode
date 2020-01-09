using System.Collections.Generic;
using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class C4TabbedHeaderRenderer
    {
        [JsonProperty("channelId")]
        public string ChannelId { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("navigationEndpoint")]
        public C4TabbedHeaderRendererNavigationEndpoint NavigationEndpoint { get; set; }

        [JsonProperty("avatar")]
        public BannerClass Avatar { get; set; }

        [JsonProperty("banner")]
        public BannerClass Banner { get; set; }

        [JsonProperty("badges")]
        public List<Badge> Badges { get; set; }

        [JsonProperty("headerLinks")]
        public HeaderLinks HeaderLinks { get; set; }

        [JsonProperty("subscribeButton")]
        public SubscribeButton SubscribeButton { get; set; }

        [JsonProperty("visitTracking")]
        public VisitTracking VisitTracking { get; set; }

        [JsonProperty("subscriberCountText")]
        public SubscriberCountText SubscriberCountText { get; set; }

        [JsonProperty("tvBanner")]
        public BannerClass TvBanner { get; set; }

        [JsonProperty("mobileBanner")]
        public BannerClass MobileBanner { get; set; }

        [JsonProperty("trackingParams")]
        public string TrackingParams { get; set; }

        [JsonProperty("sponsorButton")]
        public SponsorButton SponsorButton { get; set; }
    }
}