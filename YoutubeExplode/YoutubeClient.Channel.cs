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
        /// Gets channel information by ID.
        /// </summary>
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
                throw new ParseException("Channel does not have any videos.");

            // Get video channel
            return await GetVideoAuthorChannelAsync(video.Id).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets videos uploaded by channel with given ID.
        /// The video list is truncated at given number of pages (1 page ≤ 200 videos).
        /// </summary>
        public async Task<IReadOnlyList<Video>> GetChannelUploadsAsync(string channelId, int maxPages)
        {
            channelId.GuardNotNull(nameof(channelId));
            maxPages.GuardPositive(nameof(maxPages));
            if (!ValidateChannelId(channelId))
                throw new ArgumentException($"Invalid YouTube channel ID [{channelId}].", nameof(channelId));

            // Compose a playlist ID
            var playlistId = "UU" + channelId.SubstringAfter("UC");

            // Get playlist
            var playlist = await GetPlaylistAsync(playlistId, maxPages).ConfigureAwait(false);

            return playlist.Videos;
        }

        /// <summary>
        /// Gets videos uploaded by channel with given ID.
        /// </summary>
        public Task<IReadOnlyList<Video>> GetChannelUploadsAsync(string channelId)
            => GetChannelUploadsAsync(channelId, int.MaxValue);
    }
}