using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using YoutubeExplode.Bridge;
using YoutubeExplode.Exceptions;
using YoutubeExplode.Utils;
using YoutubeExplode.Videos;

namespace YoutubeExplode.Playlists;

internal class PlaylistController
{
    private readonly HttpClient _http;

    public PlaylistController(HttpClient http) => _http = http;

    // Works only with user-made playlists
    public async ValueTask<PlaylistBrowseResponse> GetPlaylistBrowseResponseAsync(
        PlaylistId playlistId,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/youtubei/v1/browse")
        {
            Content = Json.SerializeToHttpContent(new
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
            })
        };

        using var response = await _http.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var playlistResponse = PlaylistBrowseResponse.Parse(
            await response.Content.ReadAsStringAsync(cancellationToken)
        );

        if (!playlistResponse.IsAvailable)
            throw new PlaylistUnavailableException($"Playlist '{playlistId}' is not available.");

        return playlistResponse;
    }

    // Works on all playlists, but contains limited metadata
    public async ValueTask<PlaylistNextResponse> GetPlaylistNextResponseAsync(
        PlaylistId playlistId,
        VideoId? videoId = null,
        int index = 0,
        string? visitorData = null,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/youtubei/v1/next")
        {
            Content = Json.SerializeToHttpContent(new
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
            })
        };

        using var response = await _http.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var playlistResponse = PlaylistNextResponse.Parse(
            await response.Content.ReadAsStringAsync(cancellationToken)
        );

        if (!playlistResponse.IsAvailable)
            throw new PlaylistUnavailableException($"Playlist '{playlistId}' is not available.");

        return playlistResponse;
    }

    public async ValueTask<IPlaylistData> GetPlaylistResponseAsync(
        PlaylistId playlistId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await GetPlaylistBrowseResponseAsync(playlistId, cancellationToken);
        }
        catch (PlaylistUnavailableException)
        {
            return await GetPlaylistNextResponseAsync(playlistId, null, 0, null, cancellationToken);
        }
    }
}