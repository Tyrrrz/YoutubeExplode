using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using YoutubeExplode.Bridge;
using YoutubeExplode.Exceptions;
using YoutubeExplode.Utils;
using YoutubeExplode.Videos;

namespace YoutubeExplode.Playlists;

internal class PlaylistController : YoutubeControllerBase
{
    public PlaylistController(HttpClient http)
        : base(http)
    {
    }

    public async ValueTask<PlaylistBrowseResponseExtractor> GetPlaylistBrowseResponseAsync(
        PlaylistId playlistId,
        CancellationToken cancellationToken = default)
    {
        const string url = $"https://www.youtube.com/youtubei/v1/browse?key={ApiKey}";

        var payload = new
        {
            browseId = "VL" + playlistId,
            context = new
            {
                client = new
                {
                    clientName = "WEB",
                    clientVersion = "2.20210408.08.00",
                    hl = "en",
                    gl = "US",
                    utcOffsetMinutes = 0
                }
            }
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = Json.SerializeToHttpContent(payload)
        };

        var raw = await SendHttpRequestAsync(request, cancellationToken);
        var playlistResponse = PlaylistBrowseResponseExtractor.Create(raw);

        if (!playlistResponse.IsPlaylistAvailable())
        {
            throw new PlaylistUnavailableException($"Playlist '{playlistId}' is not available.");
        }

        return playlistResponse;
    }

    public async ValueTask<PlaylistNextResponseExtractor> GetPlaylistNextResponseAsync(
        PlaylistId playlistId,
        VideoId? videoId = null,
        int index = 0,
        string? visitorData = null,
        CancellationToken cancellationToken = default)
    {
        const string url = $"https://www.youtube.com/youtubei/v1/next?key={ApiKey}";

        var payload = new
        {
            playlistId = playlistId.Value,
            videoId = videoId?.Value,
            playlistIndex = index,
            context = new
            {
                client = new
                {
                    clientName = "WEB",
                    clientVersion = "2.20210408.08.00",
                    hl = "en",
                    gl = "US",
                    utcOffsetMinutes = 0,
                    visitorData
                }
            }
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = Json.SerializeToHttpContent(payload)
        };

        var raw = await SendHttpRequestAsync(request, cancellationToken);
        var playlistResponse = PlaylistNextResponseExtractor.Create(raw);

        if (!playlistResponse.IsPlaylistAvailable())
        {
            throw new PlaylistUnavailableException($"Playlist '{playlistId}' is not available.");
        }

        return playlistResponse;
    }

    public async ValueTask<PlaylistNextResponseExtractor> GetPlaylistNextResponseAsync(
        PlaylistId playlistId,
        CancellationToken cancellationToken = default) =>
        await GetPlaylistNextResponseAsync(playlistId, null, 0, null, cancellationToken);
}