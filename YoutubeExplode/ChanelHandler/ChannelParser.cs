using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Io.Network;
using Newtonsoft.Json;
using YoutubeExplode.ChanelHandler.ChannelPageModels;
using YoutubeExplode.ChanelHandler.Converters;
using YoutubeExplode.ChanelHandler.Exceptions;
using YoutubeExplode.ChanelHandler.YoutubeChannelPageModels;
using YoutubeExplode.Models;

namespace YoutubeExplode.ChanelHandler
{
    /// <summary>
    /// 
    /// </summary>
    public class ChannelParser
    {
        private readonly HttpClient _httpClient;
        private readonly HttpClientHandler _httpClientHandler;
        private readonly CookieContainer _cookieContainer;

        private string? _youtubeApiVersion = null;
        private DateTime _refreshedTime = DateTime.MinValue;
        private readonly TimeSpan _refreshInterval = new TimeSpan(24);
        private readonly object _lockObject = new object();

        /// <summary>
        /// Setups up the HttpClientHandler, CookieContainer and HttpClient.
        /// </summary>
        public ChannelParser()
        {
            _cookieContainer = new CookieContainer();
            _httpClientHandler = new HttpClientHandler { CookieContainer = _cookieContainer };
            if (_httpClientHandler.SupportsAutomaticDecompression)
            {
                _httpClientHandler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            }
            _httpClient = new HttpClient(_httpClientHandler);
            // This may seem strange but youtube expect a user agent otherwise they don't return page data.
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:72.0) Gecko/20100101 Firefox/72.0");
            _httpClient.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
        }

        private string GetChannelUrl(string channelId, bool jsonOnly = false)
        {
            // UCTzoGc21LVXxctEiNkIG8IQ
            if (channelId.StartsWith("UC"))
            {
                return jsonOnly ?
                    $@"https://www.youtube.com/channel/{channelId}/playlists?hl=en&pbj=1" :
                    $@"https://www.youtube.com/channel/{channelId}/playlists?hl=en";
            }
            else
            {
                return jsonOnly ?
                    $@"https://www.youtube.com/user/{channelId}/playlists?hl=en&pbj=1" :
                    $@"https://www.youtube.com/user/{channelId}/playlists?hl=en";
            }
        }


        private string GetBrowserAjaxUrl(string continuationToken)
        {
            // for some reason the continuation token is inserted twice
            return $@"https://www.youtube.com/browse_ajax?ctoken={continuationToken}&continuation={continuationToken}";
        }

        /// <summary>
        /// Obtain the playlists for a channel without continuation!
        /// </summary>
        /// <param name="channelId"></param>
        /// <param name="continuationToken"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<ChannelPlaylists?> GetPlaylistJson(string channelId, string continuationToken)
        {
            if (string.IsNullOrEmpty(continuationToken))
            {
                throw new ArgumentNullException(nameof(continuationToken));
            }

            CheckYoutubeApiVersionOrUpdate(channelId);
            var sideData = await FetchJsonFromApi(channelId, continuationToken);
            var playlistSection = sideData.Response.ContinuationContents.GridContinuation;
            if (playlistSection == null)
            {
                // TODO: custom exception
                throw new Exception("NO PLAYLIST SECTION");
            }

            if (!playlistSection.Items.Any())
            {
                return null;
            }

            string? newContinuationToken = null;
            if (playlistSection.Continuations != null)
            {
                if (playlistSection.Continuations.Count() != 1)
                {
                    // TODO: proper exception
                    throw new Exception("Continuation count is wrong");
                }
                newContinuationToken = playlistSection.Continuations.Single().NextContinuationData.Continuation;
            }

            var rawPlaylists = playlistSection.Items.Select(x => x.GridPlaylistRenderer);

            var playlists = rawPlaylists.Select(x => new ChannelPlaylist(x.PlaylistId, sideData.Response.Metadata.ChannelMetadataRenderer.Title, x.Title.Runs.First().Text, x.NavigationEndpoint.WatchEndpoint.VideoId)).ToList();

            return new ChannelPlaylists(channelId, newContinuationToken, playlists);
        }

        /// <summary>
        /// Obtain the playlists for a channel without continuation!
        /// </summary>
        /// <param name="channelId"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<ChannelPlaylists?> GetPlaylistJson(string channelId)
        {
            CheckYoutubeApiVersionOrUpdate(channelId);
            var sideData = await FetchJsonFromApi(channelId);
            var playlistSection = sideData.Contents.TwoColumnBrowseResultsRenderer.Tabs.FirstOrDefault(x => x.TabRenderer.Title == "Playlists");
            if (playlistSection == null)
            {
                // TODO: custom exception
                throw new Exception("NO PLAYLIST SECTION");
            }

            var result = playlistSection.TabRenderer.Content.SectionListRenderer.Contents.SelectMany(cp =>
                cp.ItemSectionRenderer.Contents.Select(x => x.GridRenderer));
            if (!result.Any())
            {
                return null;
            }

            var continuations = result.Where(x => x.Continuations != null).SelectMany(x => x.Continuations);
            string? continuationToken = null;
            if (result.Count() > 1)
            {
                // TODO: proper exception
                throw new Exception("Continuation count is wrong");
            }

            if (continuations.Any())
            {
                continuationToken = continuations.Single().NextContinuationData.Continuation;
            }
            var rawPlaylists = result.SelectMany(x => x.Items.Select(z => z.GridPlaylistRenderer));

            var playlists = rawPlaylists.Select(x => new ChannelPlaylist(x.PlaylistId, sideData.Header.C4TabbedHeaderRenderer.Title, x.Title.Runs.First().Text, x.NavigationEndpoint.WatchEndpoint.VideoId)).ToList();

            return new ChannelPlaylists(channelId, continuationToken, playlists);
        }

        private async Task<YoutubeAjaxResponse> FetchJsonFromApi(string channelId, string continuationToken)
        {
            var httpMessage = new HttpRequestMessage(HttpMethod.Get, GetBrowserAjaxUrl(continuationToken));
            SetRequestHeaders(httpMessage, channelId);
            var result = await _httpClient.SendAsync(httpMessage);

            if (!result.IsSuccessStatusCode)
            {
                throw new YoutubeApiCallException(channelId);
            }

            var rawJson = await result.Content.ReadAsStringAsync();

            var sideData = JsonConvert.DeserializeObject<YoutubeAjaxResponse[]>(rawJson, Converter.Settings);

            return sideData.FirstOrDefault(x => x.Response != null);
        }

        private async Task<YoutubeSideData> FetchJsonFromApi(string channelId)
        {
            var httpMessage = new HttpRequestMessage(HttpMethod.Get, GetChannelUrl(channelId, true));
            SetRequestHeaders(httpMessage, channelId);
            var result = await _httpClient.SendAsync(httpMessage);

            if (!result.IsSuccessStatusCode)
            {
                throw new YoutubeApiCallException(channelId);
            }

            var rawJson = await result.Content.ReadAsStringAsync();

            var sideData = JsonConvert.DeserializeObject<YoutubePlaylistJson[]>(rawJson, Converter.Settings);

            return sideData.FirstOrDefault(x => x?.Response != null).Response;
        }

        private async Task<YoutubeSideData> FetchJsonFromChannelPage(string channelId)
        {
            var configuration = Configuration.Default.With(new HttpClientRequester(_httpClient))
                .WithDefaultLoader();
            var bc = BrowsingContext.New(configuration);
            var document = await bc.OpenAsync(GetChannelUrl(channelId));

            var script = document.Scripts.FirstOrDefault(s => s.InnerHtml.Contains("window[\"ytInitialData\"]"));
            if (script == null)
            {
                throw new EmbedJsonNotFoundException(channelId);
            }
            var scriptLines = script.InnerHtml.Split('\n').Select(s => s.Trim()).ToList();
            var line = scriptLines.First(s => s.StartsWith("window[\"ytInitialData\"]"));
            var json = line.Substring(0, line.Length - 1).Replace("window[\"ytInitialData\"] = ", "");

            var sideData = JsonConvert.DeserializeObject<YoutubeSideData>(json, Converter.Settings);

            return sideData;
        }

        /// <summary>
        /// Find the api version from the embedded json
        /// </summary>
        /// <param name="data">Parsed embedded json</param>
        /// <returns></returns>
        private string GetYoutubeApiVersionFromJson(YoutubeSideData data)
        {
            var youtubeApiData = data.ResponseContext.ServiceTrackingParams.FirstOrDefault(x => x.Service == "ECATCHER");

            if (youtubeApiData == null)
            {
                throw new YoutubeApiVersionException();
            }

            var clientVersion = youtubeApiData.Params.FirstOrDefault(x => x.Key == "client.version");

            if (clientVersion == null)
            {
                throw new YoutubeApiVersionException();
            }

            return clientVersion.Value;
        }

        /// <summary>
        /// Used when calling the json only API
        /// </summary>
        /// <param name="message">Message to setup the headers</param>
        /// <param name="channelId">Channel we are trying to fetch for</param>
        private void SetRequestHeaders(HttpRequestMessage message, string channelId)
        {
            if (string.IsNullOrEmpty(channelId))
            {
                throw new ArgumentNullException(nameof(channelId));
            }

            message.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:72.0) Gecko/20100101 Firefox/72.0");
            message.Headers.Add("Accept", "*/*");
            message.Headers.Add("Accept-Language", new[] { "en-US", "en" });
            message.Headers.Add("Referer", $"https://www.youtube.com/user/{channelId}");

            // youtube polymer specific 
            message.Headers.Add("X-Youtube-Client-Name", "1");
            message.Headers.Add("X-YouTube-Client-Version", _youtubeApiVersion);
            message.Headers.Add("DNT", "1");
        }

        private void CheckYoutubeApiVersionOrUpdate(string channelId)
        {
            if (DateTime.UtcNow - _refreshedTime > _refreshInterval)
            {
                lock (_lockObject)
                {
                    if (DateTime.UtcNow - _refreshedTime > _refreshInterval)
                    {
                        _refreshedTime = DateTime.UtcNow;
                        var embed = FetchJsonFromChannelPage(channelId).GetAwaiter().GetResult();
                        _youtubeApiVersion = GetYoutubeApiVersionFromJson(embed);
                    }
                }
            }
        }
    }
}