using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using YoutubeExplode.Bridge.Extractors;
using YoutubeExplode.Channels;
using YoutubeExplode.Exceptions;
using YoutubeExplode.Playlists;
using YoutubeExplode.Utils;
using YoutubeExplode.Videos;

namespace YoutubeExplode.Bridge
{
    internal class YoutubeController
    {
        // This key doesn't appear to change
        private const string InternalApiKey = "AIzaSyAO_FJ2SlqU8Q4STEHLGCilw_Y9_11qcW8";

        private readonly HttpClient _httpClient;

        public YoutubeController(HttpClient httpClient) => _httpClient = httpClient;

        private async ValueTask<string> SendHttpRequestAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken = default)
        {
            // User-agent
            if (!request.Headers.Contains("User-Agent"))
            {
                request.Headers.Add(
                    "User-Agent",
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/89.0.4389.114 Safari/537.36"
                );
            }

            // Set required cookies
            request.Headers.Add("Cookie", "CONSENT=YES+cb; YSC=DwKYllHNwuw");

            using var response = await _httpClient.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken
            );

            // Special case check for rate limiting errors
            if ((int) response.StatusCode == 429)
            {
                throw new RequestLimitExceededException(
                    "Exceeded request rate limit. " +
                    "Please try again in a few hours. " +
                    "Alternatively, inject an instance of HttpClient that includes cookies for authenticated user."
                );
            }

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync(cancellationToken);
        }

        private async ValueTask<string> SendHttpRequestAsync(
            string url,
            CancellationToken cancellationToken = default)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            return await SendHttpRequestAsync(request, cancellationToken);
        }

        private async ValueTask<ChannelPageExtractor> GetChannelPageAsync(
            string channelRoute,
            CancellationToken cancellationToken = default)
        {
            var url = $"https://www.youtube.com/{channelRoute}?hl=en";

            for (var retry = 0; retry <= 5; retry++)
            {
                var raw = await SendHttpRequestAsync(url, cancellationToken);

                var channelPage = ChannelPageExtractor.TryCreate(raw);
                if (channelPage is not null)
                    return channelPage;
            }

            throw new YoutubeExplodeException(
                "Channel page is broken. " +
                "Please try again in a few minutes."
            );
        }

        public async ValueTask<ChannelPageExtractor> GetChannelPageAsync(
            ChannelId channelId,
            CancellationToken cancellationToken = default) =>
            await GetChannelPageAsync("channel/" + channelId, cancellationToken);

        public async ValueTask<ChannelPageExtractor> GetChannelPageAsync(
            UserName userName,
            CancellationToken cancellationToken = default) =>
            await GetChannelPageAsync("user/" + userName, cancellationToken);

        public async ValueTask<VideoWatchPageExtractor> GetVideoWatchPageAsync(
            VideoId videoId,
            CancellationToken cancellationToken = default)
        {
            var url = $"https://youtube.com/watch?v={videoId}&bpctr=9999999999&hl=en";

            for (var retry = 0; retry <= 5; retry++)
            {
                var raw = await SendHttpRequestAsync(url, cancellationToken);

                var watchPage = VideoWatchPageExtractor.TryCreate(raw);
                if (watchPage is not null)
                {
                    if (!watchPage.IsVideoAvailable())
                    {
                        throw new VideoUnavailableException($"Video '{videoId}' is not available.");
                    }

                    return watchPage;
                }
            }

            throw new YoutubeExplodeException(
                "Video watch page is broken. " +
                "Please try again in a few minutes."
            );
        }

        public async ValueTask<ClosedCaptionTrackExtractor> GetClosedCaptionTrackAsync(
            string url,
            CancellationToken cancellationToken = default)
        {
            // Enforce known format
            var urlWithFormat = Url.SetQueryParameter(url, "format", "3");

            var raw = await SendHttpRequestAsync(urlWithFormat, cancellationToken);

            return ClosedCaptionTrackExtractor.Create(raw);
        }

        public async ValueTask<PlayerSourceExtractor> GetPlayerSourceAsync(
            string url,
            CancellationToken cancellationToken = default)
        {
            var raw = await SendHttpRequestAsync(url, cancellationToken);
            return PlayerSourceExtractor.Create(raw);
        }

        public async ValueTask<DashManifestExtractor> GetDashManifestAsync(
            string url,
            CancellationToken cancellationToken = default)
        {
            var raw = await SendHttpRequestAsync(url, cancellationToken);
            return DashManifestExtractor.Create(raw);
        }

        public async ValueTask<PlaylistExtractor> GetPlaylistAsync(
            PlaylistId playlistId,
            string? continuationToken,
            CancellationToken cancellationToken = default)
        {
            var url = $"https://www.youtube.com/youtubei/v1/browse?key={InternalApiKey}";

            var payload = new Dictionary<string, object?>
            {
                ["browseId"] = "VL" + playlistId,
                ["continuation"] = continuationToken,
                ["context"] = new Dictionary<string, object?>
                {
                    ["client"] = new Dictionary<string, object?>
                    {
                        ["clientName"] = "WEB",
                        ["clientVersion"] = "2.20210408.08.00",
                        ["newVisitorCookie"] = true,
                        ["hl"] = "en",
                        ["gl"] = "US",
                        ["utcOffsetMinutes"] = 0
                    },
                    ["user"] = new Dictionary<string, object?>
                    {
                        ["lockedSafetyMode"] = false
                    }
                }
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = Json.SerializeToHttpContent(payload)
            };

            var raw = await SendHttpRequestAsync(request, cancellationToken);
            var playlist = PlaylistExtractor.Create(raw);

            if (!playlist.IsPlaylistAvailable())
            {
                throw new PlaylistUnavailableException($"Playlist '{playlistId}' is not available.");
            }

            return playlist;
        }

        public async ValueTask<PlaylistExtractor> GetPlaylistAsync(
            PlaylistId playlistId,
            CancellationToken cancellationToken = default) =>
            await GetPlaylistAsync(playlistId, null, cancellationToken);

        public async ValueTask<SearchResultsExtractor> GetSearchResultsAsync(
            string searchQuery,
            string? continuationToken,
            CancellationToken cancellationToken = default)
        {
            var url = $"https://www.youtube.com/youtubei/v1/search?key={InternalApiKey}";

            var payload = new Dictionary<string, object?>
            {
                ["query"] = searchQuery,
                ["continuation"] = continuationToken,
                ["context"] = new Dictionary<string, object?>
                {
                    ["client"] = new Dictionary<string, object?>
                    {
                        ["clientName"] = "WEB",
                        ["clientVersion"] = "2.20210408.08.00",
                        ["newVisitorCookie"] = true,
                        ["hl"] = "en",
                        ["gl"] = "US",
                        ["utcOffsetMinutes"] = 0
                    },
                    ["user"] = new Dictionary<string, object?>
                    {
                        ["lockedSafetyMode"] = false
                    }
                }
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = Json.SerializeToHttpContent(payload)
            };

            var raw = await SendHttpRequestAsync(request, cancellationToken);
            return SearchResultsExtractor.Create(raw);
        }
    }
}