using System.Collections.Generic;
using System.Threading.Tasks;
using YoutubeExplode.Internal.Extensions;
using YoutubeExplode.Playlists;
using YoutubeExplode.ReverseEngineering;
using YoutubeExplode.ReverseEngineering.Responses;
using YoutubeExplode.Videos;

namespace YoutubeExplode.Channels
{
    /// <summary>
    /// Queries related to YouTube channels.
    /// </summary>
    public class ChannelClient
    {
        private readonly YoutubeHttpClient _httpClient;

        /// <summary>
        /// Initializes an instance of <see cref="ChannelClient"/>.
        /// </summary>
        internal ChannelClient(YoutubeHttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        /// <summary>
        /// Gets the metadata associated with the specified channel.
        /// </summary>
        public async Task<Channel> GetAsync(ChannelId id)
        {
            var channelPage = await ChannelPage.GetAsync(_httpClient, id);

            return new Channel(
                id,
                channelPage.GetChannelTitle(),
                channelPage.GetChannelLogoUrl()
            );
        }

        /// <summary>
        /// Gets the metadata associated with the channel of the specified user.
        /// </summary>
        public async Task<Channel> GetByUserAsync(UserName userName)
        {
            var channelPage = await ChannelPage.GetByUserNameAsync(_httpClient, userName);

            return new Channel(
                channelPage.GetChannelId(),
                channelPage.GetChannelTitle(),
                channelPage.GetChannelLogoUrl()
            );
        }

        /// <summary>
        /// Gets the metadata associated with the channel that uploaded the specified video.
        /// </summary>
        public async Task<Channel> GetByVideoAsync(VideoId videoId)
        {
            var videoInfoResponse = await VideoInfoResponse.GetAsync(_httpClient, videoId);
            var playerResponse = videoInfoResponse.GetPlayerResponse();

            var channelId = playerResponse.GetVideoChannelId();

            return await GetAsync(channelId);
        }

        /// <summary>
        /// Enumerates videos uploaded by the specified channel.
        /// </summary>
        public IAsyncEnumerable<Video> GetUploadsAsync(ChannelId id)
        {
            var playlistId = "UU" + id.Value.SubstringAfter("UC");
            return new PlaylistClient(_httpClient).GetVideosAsync(playlistId);
        }
    }
}