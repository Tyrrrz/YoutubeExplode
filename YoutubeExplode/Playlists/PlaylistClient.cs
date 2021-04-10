using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using YoutubeExplode.Bridge;
using YoutubeExplode.Bridge.Extractors;

namespace YoutubeExplode.Playlists
{
    /// <summary>
    /// Queries related to YouTube playlists.
    /// </summary>
    public class PlaylistClient
    {
        private readonly YoutubeController _controller;

        internal PlaylistClient(YoutubeController controller)
        {
            _controller = controller;
        }

        /// <summary>
        /// Initializes an instance of <see cref="PlaylistClient"/>.
        /// </summary>
        public PlaylistClient(HttpClient httpClient)
            : this(new YoutubeController(httpClient))
        {
        }

        /// <summary>
        /// Gets the metadata associated with the specified playlist.
        /// </summary>
        public async ValueTask<Playlist> GetAsync(
            PlaylistId playlistId,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Enumerates the videos included in the specified playlist.
        /// </summary>
        public IAsyncEnumerable<PlaylistVideo> GetVideosAsync(
            PlaylistId playlistId,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}