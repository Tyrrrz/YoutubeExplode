using System.Net.Http;
using YoutubeExplode.Common;

namespace YoutubeExplode.Videos.Streams
{
    internal class StreamResolver : ResolverBase
    {
        public StreamResolver(HttpClient httpClient) : base(httpClient)
        {
        }
    }
}