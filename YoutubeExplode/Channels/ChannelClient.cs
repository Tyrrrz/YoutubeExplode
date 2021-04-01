using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using YoutubeExplode.Channels.Resolving;
using YoutubeExplode.Playlists;
using YoutubeExplode.Utils.Extensions;
using YoutubeExplode.Videos;

namespace YoutubeExplode.Channels
{
    /// <summary>
    /// Queries related to YouTube channels.
    /// </summary>
    public class ChannelClient
    {
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Initializes an instance of <see cref="ChannelClient"/>.
        /// </summary>
        public ChannelClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        /// <summary>
        /// Gets the metadata associated with the specified channel.
        /// </summary>
        public async ValueTask<Channel> GetAsync(ChannelId channelId)
        {
            var resolver = new ChannelResolver(_httpClient, channelId);

            return new Channel(
                channelId,
                await resolver.GetChannelTitleAsync(),
                await resolver.GetChannelLogoUrlAsync()
            );
        }

        /// <summary>
        /// Gets the metadata associated with the channel of the specified user.
        /// </summary>
        public async ValueTask<Channel> GetByUserAsync(UserName userName)
        {
            var resolver = new ChannelResolver(_httpClient, userName);

            return new Channel(
                await resolver.GetChannelIdAsync(),
                await resolver.GetChannelTitleAsync(),
                await resolver.GetChannelLogoUrlAsync()
            );
        }

        /// <summary>
        /// Gets the metadata associated with the channel that uploaded the specified video.
        /// </summary>
        public async ValueTask<Channel> GetByVideoAsync(VideoId videoId)
        {
            var video = await new VideoClient(_httpClient).GetAsync(videoId);
            return await GetAsync(video.ChannelId);
        }

        /// <summary>
        /// Enumerates the videos uploaded by the specified channel.
        /// </summary>
        public IAsyncEnumerable<PlaylistVideo> GetUploadsAsync(ChannelId channelId)
        {
            var playlistId = "UU" + channelId.Value.SubstringAfter("UC");
            return new PlaylistClient(_httpClient).GetVideosAsync(playlistId);
        }
    }
}