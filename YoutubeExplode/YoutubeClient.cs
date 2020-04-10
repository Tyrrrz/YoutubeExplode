using System;
using System.Net;
using System.Net.Http;
using YoutubeExplode.Channels;
using YoutubeExplode.Playlists;
using YoutubeExplode.ReverseEngineering;
using YoutubeExplode.Search;
using YoutubeExplode.Videos;

namespace YoutubeExplode
{
    /// <summary>
    /// Entry point for <see cref="YoutubeExplode"/>.
    /// </summary>
    public partial class YoutubeClient
    {
        public VideoClient Videos { get; }

        public PlaylistClient Playlists { get; }

        public ChannelClient Channels { get; }

        public SearchClient Search { get; }

        /// <summary>
        /// Initializes an instance of <see cref="YoutubeClient"/>.
        /// </summary>
        public YoutubeClient(HttpClient httpClient)
        {
            var youtubeHttpClient = new YoutubeHttpClient(httpClient);

            Videos = new VideoClient(youtubeHttpClient);
            Playlists = new PlaylistClient(youtubeHttpClient);
            Channels = new ChannelClient(youtubeHttpClient);
            Search = new SearchClient(youtubeHttpClient);
        }

        /// <summary>
        /// Initializes an instance of <see cref="YoutubeClient"/>.
        /// </summary>
        public YoutubeClient()
            : this(LazyHttpClient.Value)
        {
        }
    }

    public partial class YoutubeClient
    {
        private static readonly Lazy<HttpClient> LazyHttpClient = new Lazy<HttpClient>(() =>
        {
            var handler = new HttpClientHandler();

            if (handler.SupportsAutomaticDecompression)
                handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            handler.UseCookies = false;

            return new HttpClient(handler, true);
        });
    }
}