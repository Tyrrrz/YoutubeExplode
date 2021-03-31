using System.Net.Http;

namespace YoutubeExplode.Videos.Resolving
{
    internal class VideoResolver
    {
        private readonly HttpClient _httpClient;
        private readonly VideoId _videoId;

        public VideoResolver(HttpClient httpClient, VideoId videoId)
        {
            _httpClient = httpClient;
            _videoId = videoId;
        }
    }
}