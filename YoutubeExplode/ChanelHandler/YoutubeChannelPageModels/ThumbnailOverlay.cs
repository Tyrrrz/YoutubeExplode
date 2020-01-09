using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class ThumbnailOverlay
    {
        [JsonProperty("thumbnailOverlaySidePanelRenderer", NullValueHandling = NullValueHandling.Ignore)]
        public ThumbnailOverlaySidePanelRenderer ThumbnailOverlaySidePanelRenderer { get; set; }

        [JsonProperty("thumbnailOverlayHoverTextRenderer", NullValueHandling = NullValueHandling.Ignore)]
        public ThumbnailOverlayHoverTextRenderer ThumbnailOverlayHoverTextRenderer { get; set; }

        [JsonProperty("thumbnailOverlayNowPlayingRenderer", NullValueHandling = NullValueHandling.Ignore)]
        public ThumbnailOverlayNowPlayingRenderer ThumbnailOverlayNowPlayingRenderer { get; set; }
    }
}