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
            channelId.EnsureNotNull(nameof(channelId));
            if (!ValidateChannelId(channelId))
                throw new ArgumentException("Invalid Youtube channel ID", nameof(channelId));

            // HACK: this needs to be reworked somehow

            // Get channel uploads
            var uploads = await GetChannelUploadsAsync(channelId, 1).ConfigureAwait(false);

            // Get first video
            var playlistVideo = uploads.FirstOrDefault();
            if (playlistVideo == null)
                throw new ParseException("Cannot get channel info because it doesn't have any uploaded videos");

            // Get full video info
            var video = await GetVideoAsync(playlistVideo.Id).ConfigureAwait(false);

            return video.Author;
        }

        /// <summary>
        /// Gets videos uploaded to a channel with given ID, truncating resulting video list at given number of pages (1 page ≤ 200 videos)
        /// </summary>
        public async Task<IReadOnlyList<PlaylistVideo>> GetChannelUploadsAsync(string channelId, int maxPages)
        {
            channelId.EnsureNotNull(nameof(channelId));
            maxPages.EnsurePositive(nameof(maxPages));
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