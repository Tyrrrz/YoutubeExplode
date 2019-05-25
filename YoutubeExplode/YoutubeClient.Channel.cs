﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AngleSharp.Dom.Html;
using AngleSharp.Parser.Html;
using YoutubeExplode.Internal;
using YoutubeExplode.Models;

namespace YoutubeExplode
{
    public partial class YoutubeClient
    {
        private async Task<IHtmlDocument> GetUserPageHtmlAsync(string username)
        {
            var url = $"https://www.youtube.com/user/{username}?hl=en";
            var raw = await _httpClient.GetStringAsync(url).ConfigureAwait(false);

            return new HtmlParser().Parse(raw);
        }

        private async Task<IHtmlDocument> GetChannelPageHtmlAsync(string channelId)
        {
            var url = $"https://www.youtube.com/channel/{channelId}?hl=en";
            var raw = await _httpClient.GetStringAsync(url).ConfigureAwait(false);

            return new HtmlParser().Parse(raw);
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
            var channelUrl = userPageHtml.QuerySelector("meta[property=\"og:url\"]").GetAttribute("content");

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
            var playlist = await GetPlaylistAsync(playlistId, maxPages).ConfigureAwait(false);

            return playlist.Videos;
        }

        /// <inheritdoc />
        public Task<IReadOnlyList<Video>> GetChannelUploadsAsync(string channelId) => GetChannelUploadsAsync(channelId, int.MaxValue);
    }
}