using System;
using System.Collections.Generic;
using System.Linq;
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

            // Get parser
            var parser = await GetChannelPageParserByUsernameAsync(username).ConfigureAwait(false);

            // Parse info
            var channelId = parser.ParseChannelId();

            return channelId;
        }

        /// <inheritdoc />
        public async Task<Channel> GetChannelAsync(string channelId)
        {
            channelId.GuardNotNull(nameof(channelId));

            if (!ValidateChannelId(channelId))
                throw new ArgumentException($"Invalid YouTube channel ID [{channelId}].", nameof(channelId));

            // This is a hack, it gets uploads and then gets uploader info of first video

            // Get channel uploads
            var uploads = await GetChannelUploadsAsync(channelId, 1).ConfigureAwait(false);

            // Get first video
            var video = uploads.FirstOrDefault();
            if (video == null)
                throw new InvalidOperationException("Channel contains no videos.");

            // Get video channel
            var channel = await GetVideoAuthorChannelAsync(video.Id).ConfigureAwait(false);

            return channel;
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<Video>> GetChannelUploadsAsync(string channelId, int maxPages)
        {
            channelId.GuardNotNull(nameof(channelId));
            maxPages.GuardPositive(nameof(maxPages));

            if (!ValidateChannelId(channelId))
                throw new ArgumentException($"Invalid YouTube channel ID [{channelId}].", nameof(channelId));

            // Compose ID for the playlist that contains all videos uploaded by this channel
            var playlistId = "UU" + channelId.SubstringAfter("UC");

            // Get playlist
            var playlist = await GetPlaylistAsync(playlistId, maxPages).ConfigureAwait(false);

            return playlist.Videos;
        }

        /// <inheritdoc />
        public Task<IReadOnlyList<Video>> GetChannelUploadsAsync(string channelId)
            => GetChannelUploadsAsync(channelId, int.MaxValue);
    }
}