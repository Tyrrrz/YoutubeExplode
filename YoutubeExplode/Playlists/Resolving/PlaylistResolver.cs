using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace YoutubeExplode.Playlists.Resolving
{
    internal class PlaylistResolver
    {
        private readonly HttpClient _httpClient;
        private readonly PlaylistId _playlistId;

        public PlaylistResolver(HttpClient httpClient, PlaylistId playlistId)
        {
            _httpClient = httpClient;
            _playlistId = playlistId;
        }

        private async ValueTask<JsonElement> GetResponseAsync()
        {

        }
    }
}