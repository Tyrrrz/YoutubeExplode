using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using YoutubeExplode.ChanelHandler.Converters;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class MicroformatDataRenderer
    {
        [JsonProperty("urlCanonical")]
        public Uri UrlCanonical { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("thumbnail")]
        public PurpleAvatar Thumbnail { get; set; }

        [JsonProperty("siteName")]
        public string SiteName { get; set; }

        [JsonProperty("appName")]
        public string AppName { get; set; }

        [JsonProperty("androidPackage")]
        public string AndroidPackage { get; set; }

        [JsonProperty("iosAppStoreId")]
        [JsonConverter(typeof(ParseStringConverter))]
        public long IosAppStoreId { get; set; }

        [JsonProperty("iosAppArguments")]
        public Uri IosAppArguments { get; set; }

        [JsonProperty("ogType")]
        public string OgType { get; set; }

        [JsonProperty("urlApplinksWeb")]
        public Uri UrlApplinksWeb { get; set; }

        [JsonProperty("urlApplinksIos")]
        public string UrlApplinksIos { get; set; }

        [JsonProperty("urlApplinksAndroid")]
        public string UrlApplinksAndroid { get; set; }

        [JsonProperty("urlTwitterIos")]
        public string UrlTwitterIos { get; set; }

        [JsonProperty("urlTwitterAndroid")]
        public string UrlTwitterAndroid { get; set; }

        [JsonProperty("twitterCardType")]
        public string TwitterCardType { get; set; }

        [JsonProperty("twitterSiteHandle")]
        public string TwitterSiteHandle { get; set; }

        [JsonProperty("schemaDotOrgType")]
        public Uri SchemaDotOrgType { get; set; }

        [JsonProperty("noindex")]
        public bool Noindex { get; set; }

        [JsonProperty("unlisted")]
        public bool Unlisted { get; set; }

        [JsonProperty("tags")]
        public List<string> Tags { get; set; }

        [JsonProperty("linkAlternates")]
        public List<LinkAlternate> LinkAlternates { get; set; }
    }
}