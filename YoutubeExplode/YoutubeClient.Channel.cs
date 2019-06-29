using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LtGt;
using LtGt.Models;
using YoutubeExplode.Internal;
using YoutubeExplode.Models;

namespace YoutubeExplode
{
    public partial class YoutubeClient
    {
        private async Task<HtmlDocument> GetUserPageHtmlAsync(string username)
        {
            var url = $"https://www.youtube.com/user/{username}?hl=en";
            var raw = await _httpClient.GetStringAsync(url).ConfigureAwait(false);

            return HtmlParser.Default.ParseDocument(raw);
        }

        private async Task<HtmlDocument> GetChannelPageHtmlAsync(string channelId)
        {
            var url = $"https://www.youtube.com/channel/{channelId}?hl=en";
            var raw = await _httpClient.GetStringAsync(url).ConfigureAwait(false);

            return HtmlParser.Default.ParseDocument(raw);
        }

        /// <inheritdoc />
        public async Task<string> GetChannelIdAsync(string username)
        {
            username.GuardNotNull(nameof(username));

            if (!ValidateUsername(username))
                throw new ArgumentException($"Invalid YouTube username [{username}].");

            // Get user page HTML
            var userPageHtml = await GetUserPageHtmlAsync(username).ConfigureAwait(false);

            // Extract channel URL
            var channelUrl = userPageHtml.GetElementsByTagName("meta")
                .First(e => e.GetAttribute("property")?.Value == "og:url")
                .GetAttribute("content")?.Value;

            return channelUrl.SubstringAfter("channel/");
        }

        /// <inheritdoc />
        public async Task<Channel> GetChannelAsync(string channelId)
        {
            channelId.GuardNotNull(nameof(channelId));

            if (!ValidateChannelId(channelId))
                throw new ArgumentException($"Invalid YouTube channel ID [{channelId}].", nameof(channelId));

            // Get channel page HTML
            var channelPageHtml = await GetChannelPageHtmlAsync(channelId).ConfigureAwait(false);

            // Extract info
            var channelTitle = channelPageHtml.GetElementsByTagName("meta")
                .First(e => e.GetAttribute("property")?.Value == "og:title")
                .GetAttribute("content")?.Value;

            var channelLogoUrl = channelPageHtml.GetElementsByTagName("meta")
                .First(e => e.GetAttribute("property")?.Value == "og:image")
                .GetAttribute("content")?.Value;

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
            var playlist = await GetPlaylistAsync(playlistId, maxPages).ConfigureAwait(false);

            return playlist.Videos;
        }

        /// <inheritdoc />
        public Task<IReadOnlyList<Video>> GetChannelUploadsAsync(string channelId) => GetChannelUploadsAsync(channelId, int.MaxValue);
    }
}