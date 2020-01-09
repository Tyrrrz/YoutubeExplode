using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class ChannelMetadataRenderer
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("rssUrl")]
        public Uri RssUrl { get; set; }

        [JsonProperty("plusPageLink")]
        public Uri PlusPageLink { get; set; }

        [JsonProperty("channelConversionUrl")]
        public Uri ChannelConversionUrl { get; set; }

        [JsonProperty("externalId")]
        public string ExternalId { get; set; }

        [JsonProperty("doubleclickTrackingUsername")]
        public string DoubleclickTrackingUsername { get; set; }

        [JsonProperty("keywords")]
        public string Keywords { get; set; }

        [JsonProperty("ownerUrls")]
        public List<Uri> OwnerUrls { get; set; }

        [JsonProperty("avatar")]
        public PurpleAvatar Avatar { get; set; }

        [JsonProperty("channelUrl")]
        public Uri ChannelUrl { get; set; }

        [JsonProperty("isFamilySafe")]
        public bool IsFamilySafe { get; set; }

        [JsonProperty("facebookProfileId")]
        public string FacebookProfileId { get; set; }

        [JsonProperty("availableCountryCodes")]
        public List<string> AvailableCountryCodes { get; set; }

        [JsonProperty("analyticsId")]
        public string AnalyticsId { get; set; }

        [JsonProperty("androidDeepLink")]
        public string AndroidDeepLink { get; set; }

        [JsonProperty("androidAppindexingLink")]
        public string AndroidAppindexingLink { get; set; }

        [JsonProperty("iosAppindexingLink")]
        public string IosAppindexingLink { get; set; }

        [JsonProperty("tabPath")]
        public string TabPath { get; set; }

        [JsonProperty("vanityChannelUrl")]
        public Uri VanityChannelUrl { get; set; }
    }
}