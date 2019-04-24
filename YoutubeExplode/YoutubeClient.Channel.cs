using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using YoutubeExplode.Internal;
using YoutubeExplode.Models;

namespace YoutubeExplode
{
    public partial class YoutubeClient
    {
        /// <inheritdoc />
        public async Task<string> GetChannelIdAsync(string username)
        {
            username.GuardNotNull(nameof(username));

            if (!ValidateUsername(username))
                throw new ArgumentException($"Invalid YouTube username [{username}].");

            // Get channel page parser
            var channelPageParser = await GetChannelPageParserForUserAsync(username);

            return channelPageParser.GetChannelId();
        }

        /// <inheritdoc />
        public async Task<Channel> GetChannelAsync(string channelId)
        {
            channelId.GuardNotNull(nameof(channelId));

            if (!ValidateChannelId(channelId))
                throw new ArgumentException($"Invalid YouTube channel ID [{channelId}].", nameof(channelId));

            // Get channel page parser
            var channelPageParser = await GetChannelPageParserAsync(channelId);

            // Extract info
            var channelTitle = channelPageParser.GetChannelTitle();
            var channelLogoUrl = channelPageParser.GetChannelLogoUrl();

            return new Channel(channelId, channelTitle, channelLogoUrl);
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<Video>> GetChannelUploadsAsync(string channelId, int maxPages)
        {
            channelId.GuardNotNull(nameof(channelId));
            maxPages.GuardPositive(nameof(maxPages));

            if (!ValidateChannelId(channelId))
                throw new ArgumentException($"Invalid YouTube channel ID [{channelId}].", nameof(channelId));

            // Generate ID for the playlist that contains all videos uploaded by this channel
            var playlistId = "UU" + channelId.SubstringAfter("UC");

            // Get playlist
            var playlist = await GetPlaylistAsync(playlistId, maxPages);

            return playlist.Videos;
        }

        /// <inheritdoc />
        public Task<IReadOnlyList<Video>> GetChannelUploadsAsync(string channelId) => GetChannelUploadsAsync(channelId, int.MaxValue);
    }
}