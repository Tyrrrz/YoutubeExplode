using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using YoutubeExplode.Bridge;
using YoutubeExplode.Exceptions;
using YoutubeExplode.Playlists;

namespace YoutubeExplode.Channels
{
    /// <summary>
    /// Operations related to YouTube channels.
    /// </summary>
    public class ChannelClient
    {
        private readonly YoutubeController _controller;

        /// <summary>
        /// Initializes an instance of <see cref="ChannelClient"/>.
        /// </summary>
        public ChannelClient(HttpClient httpClient)
        {
            _controller = new YoutubeController(httpClient);
        }

        /// <summary>
        /// Gets the metadata associated with the specified channel.
        /// </summary>
        public async ValueTask<Channel> GetAsync(
            ChannelId channelId,
            CancellationToken cancellationToken = default)
        {
            var channelPage = await _controller.GetChannelPageAsync(channelId, cancellationToken);

            var title =
                channelPage.TryGetChannelTitle() ??
                throw new YoutubeExplodeException("Could not extract channel title.");

            var logoUrl =
                channelPage.TryGetChannelLogoUrl() ??
                throw new YoutubeExplodeException("Could not extract channel logo URL.");

            return new Channel(
                channelId,
                title,
                logoUrl
            );
        }

        /// <summary>
        /// Gets the metadata associated with the channel of the specified user.
        /// </summary>
        public async ValueTask<Channel> GetByUserAsync(
            UserName userName,
            CancellationToken cancellationToken = default)
        {
            var channelPage = await _controller.GetChannelPageAsync(userName, cancellationToken);

            var channelId =
                channelPage.TryGetChannelId() ??
                throw new YoutubeExplodeException("Could not extract channel ID.");

            var title =
                channelPage.TryGetChannelTitle() ??
                throw new YoutubeExplodeException("Could not extract channel title.");

            var logoUrl =
                channelPage.TryGetChannelLogoUrl() ??
                throw new YoutubeExplodeException("Could not extract channel logo URL.");

            return new Channel(
                channelId,
                title,
                logoUrl
            );
        }

        /// <summary>
        /// Enumerates videos uploaded by the specified channel.
        /// </summary>
        public IAsyncEnumerable<PlaylistVideo> GetUploadsAsync(
            ChannelId channelId,
            CancellationToken cancellationToken = default)
        {
            // Replace 'UC' in channel ID with 'UU'
            var playlistId = "UU" + channelId.Value.Substring(2);

            return new PlaylistClient(_controller).GetVideosAsync(playlistId, cancellationToken);
        }
    }
}