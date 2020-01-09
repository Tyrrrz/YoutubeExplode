using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.JsonApiModels
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
}
