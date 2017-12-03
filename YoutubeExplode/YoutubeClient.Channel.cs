using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YoutubeExplode.Exceptions;
using YoutubeExplode.Internal;
using YoutubeExplode.Models;

namespace YoutubeExplode
{
    public partial class YoutubeClient
    {
        /// <summary>
        /// Gets channel by ID
        /// </summary>
        public async Task<Channel> GetChannelAsync(string channelId)
        {
            channelId.GuardNotNull(nameof(channelId));
            if (!ValidateChannelId(channelId))
                throw new ArgumentException("Invalid Youtube channel ID", nameof(channelId));

            // This is a hack

            // Get channel uploads
            var uploads = await GetChannelUploadsAsync(channelId, 1).ConfigureAwait(false);

            // Get first video
            var video = uploads.FirstOrDefault();
            if (video == null)
                throw new ParseException("Channel does not have any videos");

            // Get video channel
            return await GetVideoChannelAsync(video.Id).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets videos uploaded to a channel with given ID, truncating resulting video list at given number of pages (1 page ≤ 200 videos)
        /// </summary>
        public async Task<IReadOnlyList<PlaylistVideo>> GetChannelUploadsAsync(string channelId, int maxPages)
        {
            channelId.GuardNotNull(nameof(channelId));
            maxPages.GuardPositive(nameof(maxPages));
            if (!ValidateChannelId(channelId))
                throw new ArgumentException("Invalid Youtube channel ID", nameof(channelId));

            // Compose a playlist ID
            var playlistId = "UU" + channelId.SubstringAfter("UC");

            // Get playlist
            var playlist = await GetPlaylistAsync(playlistId, maxPages).ConfigureAwait(false);

            return playlist.Videos;
        }

        /// <summary>
        /// Gets videos uploaded to a channel with given ID
        /// </summary>
        public Task<IReadOnlyList<PlaylistVideo>> GetChannelUploadsAsync(string channelId)
            => GetChannelUploadsAsync(channelId, int.MaxValue);
    }
}