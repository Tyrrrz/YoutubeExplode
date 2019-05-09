using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AngleSharp.Parser.Html;
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

            // Get channel page
            var url = $"https://www.youtube.com/user/{username}?hl=en";
            var channelPageRaw = await _httpClient.GetStringAsync(url);
            var channelPageHtml = new HtmlParser().Parse(channelPageRaw);

            // Get channel URL
            var channelUrl = channelPageHtml.QuerySelector("meta[property=\"og:url\"]").GetAttribute("content");

            return channelUrl.SubstringAfter("channel/");
        }

        /// <inheritdoc />
        public async Task<Channel> GetChannelAsync(string channelId)
        {
            channelId.GuardNotNull(nameof(channelId));

            if (!ValidateChannelId(channelId))
                throw new ArgumentException($"Invalid YouTube channel ID [{channelId}].", nameof(channelId));

            // Get channel page
            var url = $"https://www.youtube.com/channel/{channelId}?hl=en";
            var channelPageRaw = await _httpClient.GetStringAsync(url);
            var channelPageHtml = new HtmlParser().Parse(channelPageRaw);

            // Extract info
            var channelTitle = channelPageHtml.QuerySelector("meta[property=\"og:title\"]").GetAttribute("content");
            var channelLogoUrl = channelPageHtml.QuerySelector("meta[property=\"og:image\"]").GetAttribute("content");

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