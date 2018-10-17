using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp.Dom.Html;
using AngleSharp.Parser.Html;
using YoutubeExplode.Exceptions;
using YoutubeExplode.Internal;
using YoutubeExplode.Models;

namespace YoutubeExplode
{
    public partial class YoutubeClient
    {
        private async Task<string> GetUserPageRawAsync(string username)
        {
            username = username.UrlEncode();
            var url = $"https://www.youtube.com/user/{username}";
            return await _httpClient.GetStringAsync(url, false).ConfigureAwait(false);
        }

        private async Task<IHtmlDocument> GetUserPageAsync(string username)
        {
            var raw = await GetUserPageRawAsync(username).ConfigureAwait(false);
            return await new HtmlParser().ParseAsync(raw).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<string> GetChannelIdAsync(string username)
        {
            username.GuardNotNull(nameof(username));

            // Get user page
            var userPage = await GetUserPageAsync(username).ConfigureAwait(false);

            // Extract channel ID
            var channelId = userPage.QuerySelector("link[rel=\"canonical\"]").GetAttribute("href")
                .SubstringAfter("channel/");

            // Validate channel ID
            if (!ValidateChannelId(channelId))
                throw new ParseException("Could not parse channel ID.");

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
                throw new ParseException("Channel does not have any videos.");

            // Get video channel
            return await GetVideoAuthorChannelAsync(video.Id).ConfigureAwait(false);
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public Task<IReadOnlyList<Video>> GetChannelUploadsAsync(string channelId)
            => GetChannelUploadsAsync(channelId, int.MaxValue);
    }
}