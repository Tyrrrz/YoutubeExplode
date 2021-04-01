using System.Net.Http;
using YoutubeExplode.Channels;
using YoutubeExplode.Playlists;
using YoutubeExplode.Search;
using YoutubeExplode.Utils;
using YoutubeExplode.Videos;

namespace YoutubeExplode
{
    /// <summary>
    /// Client for interacting with YouTube.
    /// </summary>
    public class YoutubeClient
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
        public YoutubeClient(HttpClient httpClient)
        {
            Videos = new VideoClient(httpClient);
            Playlists = new PlaylistClient(httpClient);
            Channels = new ChannelClient(httpClient);
            Search = new SearchClient(httpClient);
        }

        /// <summary>
        /// Initializes an instance of <see cref="YoutubeClient"/>.
        /// </summary>
        public YoutubeClient() : this(Http.Client)
        {
        }
    }
}