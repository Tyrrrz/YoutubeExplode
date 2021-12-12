using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using YoutubeExplode.Bridge;
using YoutubeExplode.Exceptions;
using YoutubeExplode.Utils;

namespace YoutubeExplode.Playlists;

internal class PlaylistController : YoutubeControllerBase
{
    public PlaylistController(HttpClient http)
        : base(http)
    {
    }

    public async ValueTask<PlaylistExtractor> GetPlaylistAsync(
        PlaylistId playlistId,
        string? continuationToken,
        CancellationToken cancellationToken = default)
    {
        const string url = $"https://www.youtube.com/youtubei/v1/browse?key={ApiKey}";

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
                    ["hl"] = "en",
                    ["gl"] = "US",
                    ["utcOffsetMinutes"] = 0
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
}