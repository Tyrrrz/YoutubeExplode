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
        var raw = await GetStringAsync(() => new HttpRequestMessage(HttpMethod.Post, "/youtubei/v1/browse")
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
        }, cancellationToken);

        var playlistResponse = PlaylistBrowseResponseExtractor.Create(raw);

        if (!playlistResponse.IsPlaylistAvailable())
            throw new PlaylistUnavailableException($"Playlist '{playlistId}' is not available.");

        return playlistResponse;
    }

    public async ValueTask<PlaylistNextResponseExtractor> GetPlaylistNextResponseAsync(
        PlaylistId playlistId,
        VideoId? videoId = null,
        int index = 0,
        string? visitorData = null,
        CancellationToken cancellationToken = default)
    {
        var raw = await GetStringAsync(() => new HttpRequestMessage(HttpMethod.Post, "/youtubei/v1/next")
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
        }, cancellationToken);

        var playlistResponse = PlaylistNextResponseExtractor.Create(raw);

        if (!playlistResponse.IsPlaylistAvailable())
            throw new PlaylistUnavailableException($"Playlist '{playlistId}' is not available.");

        return playlistResponse;
    }

    public async ValueTask<PlaylistNextResponseExtractor> GetPlaylistNextResponseAsync(
        PlaylistId playlistId,
        CancellationToken cancellationToken = default) =>
        await GetPlaylistNextResponseAsync(playlistId, null, 0, null, cancellationToken);
}