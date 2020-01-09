using System.Collections.Generic;
using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class GridPlaylistRenderer
    {
        [JsonProperty("playlistId")]
        public string PlaylistId { get; set; }

        [JsonProperty("thumbnail")]
        public BannerClass Thumbnail { get; set; }

        [JsonProperty("title")]
        public Title Title { get; set; }

        [JsonProperty("videoCountText")]
        public SubscriberCountText VideoCountText { get; set; }

        [JsonProperty("navigationEndpoint")]
        public GridPlaylistRendererNavigationEndpoint NavigationEndpoint { get; set; }

        [JsonProperty("publishedTimeText")]
        public ShortSubscriberCountText PublishedTimeText { get; set; }

        [JsonProperty("videoCountShortText")]
        public ShortSubscriberCountText VideoCountShortText { get; set; }

        [JsonProperty("trackingParams")]
        public string TrackingParams { get; set; }

        [JsonProperty("sidebarThumbnails", NullValueHandling = NullValueHandling.Ignore)]
        public List<PurpleAvatar> SidebarThumbnails { get; set; }

        [JsonProperty("thumbnailText")]
        public ThumbnailText ThumbnailText { get; set; }

        [JsonProperty("ownerBadges")]
        public List<Badge> OwnerBadges { get; set; }

        [JsonProperty("thumbnailRenderer")]
        public ThumbnailRenderer ThumbnailRenderer { get; set; }

        [JsonProperty("thumbnailOverlays")]
        public List<ThumbnailOverlay> ThumbnailOverlays { get; set; }

        [JsonProperty("viewPlaylistText")]
        public ViewPlaylistText ViewPlaylistText { get; set; }
    }
}