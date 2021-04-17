using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using YoutubeExplode.Bridge;
using YoutubeExplode.Common;
using YoutubeExplode.Exceptions;
using YoutubeExplode.Playlists;
using YoutubeExplode.Utils.Extensions;

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

        // TODO:
        private IReadOnlyList<Thumbnail> GenerateThumbnails(string logoUrl)
        {
            var logoSize = Regex
                .Matches(logoUrl, @"\bs(\d+)\b")
                .Cast<Match>()
                .LastOrDefault()?
                .Groups[1]
                .Value
                .NullIfWhiteSpace()?
                .ParseIntOrNull() ?? 100;

            return new[]
            {
                new Thumbnail(logoUrl, new Resolution(logoSize, logoSize))
            };
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

            var thumbnails = GenerateThumbnails(logoUrl);

            return new Channel(
                channelId,
                title,
                thumbnails
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

            var thumbnails = GenerateThumbnails(logoUrl);

            return new Channel(
                channelId,
                title,
                thumbnails
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