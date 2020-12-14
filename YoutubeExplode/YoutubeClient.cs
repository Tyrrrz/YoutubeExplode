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
        /// <summary>
        /// Queries related to YouTube videos.
        /// </summary>
        public VideoClient Videos { get; }

        /// <summary>
        /// Queries related to YouTube playlists.
        /// </summary>
        public PlaylistClient Playlists { get; }

        /// <summary>
        /// Queries related to YouTube channels.
        /// </summary>
        public ChannelClient Channels { get; }

        /// <summary>
        /// YouTube search queries.
        /// </summary>
        public SearchClient Search { get; }

        /// <summary>
        /// Initializes an instance of <see cref="YoutubeClient"/>.
        /// </summary>
        internal YoutubeClient(YoutubeHttpClient httpClient)
        {
            Videos = new VideoClient(httpClient);
            Playlists = new PlaylistClient(httpClient);
            Channels = new ChannelClient(httpClient);
            Search = new SearchClient(httpClient);
        }

        /// <summary>
        /// Initializes an instance of <see cref="YoutubeClient"/>.
        /// </summary>
        public YoutubeClient(HttpClient httpClient)
            : this(new YoutubeHttpClient(httpClient))
        {
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
        private static readonly Lazy<HttpClient> LazyHttpClient = new(() =>
        {
            var handler = new HttpClientHandler();

            if (handler.SupportsAutomaticDecompression)
                handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            var httpClient = new HttpClient(handler, true);

            httpClient.DefaultRequestHeaders.Add(
                "User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/86.0.4240.111 Safari/537.36"
            );

            return httpClient;
        });
    }
}