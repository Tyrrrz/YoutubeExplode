using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using YoutubeExplode.Bridge;
using YoutubeExplode.Exceptions;
using YoutubeExplode.Utils;
using YoutubeExplode.Videos;

namespace YoutubeExplode.Playlists;

internal class PlaylistController(HttpClient http)
{
    // Works only with user-made playlists
    public async ValueTask<PlaylistBrowseResponse> GetPlaylistBrowseResponseAsync(
        PlaylistId playlistId,
        CancellationToken cancellationToken = default
    )
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            "https://www.youtube.com/youtubei/v1/browse"
        );

        request.Content = new StringContent(
            // lang=json
            $$"""
            {
              "browseId": {{Json.Serialize("VL" + playlistId)}},
              "context": {
                "client": {
                  "clientName": "WEB",
                  "clientVersion": "2.20210408.08.00",
                  "hl": "en",
                  "gl": "US",
                  "utcOffsetMinutes": 0
                }
              }
            }
            """
        );

        using var response = await http.SendAsync(request, cancellationToken);
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
        CancellationToken cancellationToken = default
    )
    {
        const int retriesCount = 5;
        for (var retriesRemaining = retriesCount; ; retriesRemaining--)
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                "https://www.youtube.com/youtubei/v1/next"
            );

            request.Content = new StringContent(
                // lang=json
                $$"""
                {
                  "playlistId": {{Json.Serialize(playlistId)}},
                  "videoId": {{Json.Serialize(videoId)}},
                  "playlistIndex": {{Json.Serialize(index)}},
                  "context": {
                    "client": {
                      "clientName": "WEB",
                      "clientVersion": "2.20210408.08.00",
                      "hl": "en",
                      "gl": "US",
                      "utcOffsetMinutes": 0,
                      "visitorData": {{Json.Serialize(visitorData)}}
                    }
                  }
                }
                """
            );

            using var response = await http.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var playlistResponse = PlaylistNextResponse.Parse(
                await response.Content.ReadAsStringAsync(cancellationToken)
            );

            if (!playlistResponse.IsAvailable)
            {
                // Retry if this is not the first request, meaning that the previous requests were successful,
                // indicating that it's most likely a transient error.
                if (index > 0 && !string.IsNullOrWhiteSpace(visitorData) && retriesRemaining > 0)
                    continue;

                // Some system playlists are unavailable through this endpoint until their page is opened by
                // at least one user. If this is the first request, and we haven't retried yet, attempt to
                // warm up the playlist by opening its page, and then retry.
                if (
                    index <= 0
                    && string.IsNullOrWhiteSpace(visitorData)
                    && retriesRemaining >= retriesCount
                )
                {
                    using (
                        await http.GetAsync(
                            $"https://youtube.com/playlist?list={playlistId}",
                            cancellationToken
                        )
                    )
                    {
                        // We don't actually care about the outcome of this request
                    }

                    continue;
                }

                throw new PlaylistUnavailableException(
                    $"Playlist '{playlistId}' is not available."
                );
            }

            return playlistResponse;
        }
    }

    public async ValueTask<IPlaylistData> GetPlaylistResponseAsync(
        PlaylistId playlistId,
        CancellationToken cancellationToken = default
    )
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
