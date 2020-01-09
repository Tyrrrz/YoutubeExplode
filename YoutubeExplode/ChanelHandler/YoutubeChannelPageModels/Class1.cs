using YoutubeExplode.ChanelHandler.ChannelPageModels;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.YoutubeChannelPageModels
{
    internal class YoutubeAjaxResponse
    {
        [JsonProperty("csn")]
        public string Csn { get; set; }

        [JsonProperty("page")]
        public string Page { get; set; }

        [JsonProperty("response", NullValueHandling = NullValueHandling.Ignore)]
        public Response Response { get; set; }

        [JsonProperty("endpoint", NullValueHandling = NullValueHandling.Ignore)]
        public Endpoint Endpoint { get; set; }

        [JsonProperty("xsrf_token", NullValueHandling = NullValueHandling.Ignore)]
        public string XsrfToken { get; set; }

        [JsonProperty("timing", NullValueHandling = NullValueHandling.Ignore)]
        public Timing Timing { get; set; }
    }

    internal class Endpoint
    {
        [JsonProperty("commandMetadata")]
        public CommandMetadata CommandMetadata { get; set; }

        [JsonProperty("urlEndpoint")]
        public UrlEndpoint UrlEndpoint { get; set; }
    }

    internal class CommandMetadata
    {
        [JsonProperty("webCommandMetadata")]
        public WebCommandMetadata WebCommandMetadata { get; set; }
    }

    internal class WebCommandMetadata
    {
        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("webPageType")]
        public string WebPageType { get; set; }

        [JsonProperty("rootVe")]
        public long RootVe { get; set; }
    }

    internal class UrlEndpoint
    {
        [JsonProperty("url")]
        public string Url { get; set; }
    }

    internal class Response
    {
        [JsonProperty("responseContext")]
        public ResponseContext ResponseContext { get; set; }

        [JsonProperty("trackingParams")]
        public string TrackingParams { get; set; }

        [JsonProperty("metadata")]
        public Metadata Metadata { get; set; }

        [JsonProperty("continuationContents")]
        public ContinuationContents ContinuationContents { get; set; }

        [JsonProperty("microformat")]
        public Microformat Microformat { get; set; }
    }

    internal class ContinuationContents
    {
        [JsonProperty("gridContinuation")]
        public GridRenderer GridContinuation { get; set; }
    }

    internal class ServiceTrackingParam
    {
        [JsonProperty("service")]
        public string Service { get; set; }

        [JsonProperty("params")]
        public List<Param> Params { get; set; }
    }

    internal class Param
    {
        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }
    }

    internal class WebResponseContextExtensionData
    {
        [JsonProperty("webResponseContextPreloadData")]
        public WebResponseContextPreloadData WebResponseContextPreloadData { get; set; }

        [JsonProperty("ytConfigData")]
        public YtConfigData YtConfigData { get; set; }
    }

    internal class WebResponseContextPreloadData
    {
        [JsonProperty("preloadThumbnailUrls")]
        public List<Uri> PreloadThumbnailUrls { get; set; }
    }

    internal class YtConfigData
    {
        [JsonProperty("csn")]
        public string Csn { get; set; }

        [JsonProperty("visitorData")]
        public string VisitorData { get; set; }
    }

    internal class Timing
    {
        [JsonProperty("info")]
        public Info Info { get; set; }
    }

    internal class Info
    {
        [JsonProperty("st")]
        public long St { get; set; }
    }
}
