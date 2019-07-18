using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YoutubeExplode.Internal;
using YoutubeExplode.Models;

#if NETSTANDARD1_1
using LtGt;
using LtGt.Models;
#else
using HtmlAgilityPack;
#endif


namespace YoutubeExplode
{
    public partial class YoutubeClient
    {
        private async Task<HtmlDocument> GetUserPageHtmlAsync(string username)
        {
            var url = $"https://www.youtube.com/user/{username}?hl=en";
            var raw = await _httpClient.GetStringAsync(url).ConfigureAwait(false);

#if NETSTANDARD1_1
            return HtmlParser.Default.ParseDocument(raw);
#else
            var doc = new HtmlDocument();
            doc.LoadHtml(raw);
            return doc;
#endif
        }

        private async Task<HtmlDocument> GetChannelPageHtmlAsync(string channelId)
        {
            var url = $"https://www.youtube.com/channel/{channelId}?hl=en";
            var raw = await _httpClient.GetStringAsync(url).ConfigureAwait(false);

#if NETSTANDARD1_1
            return HtmlParser.Default.ParseDocument(raw);
#else
            var doc = new HtmlDocument();
            doc.LoadHtml(raw);
            return doc;
#endif
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
#if NETSTANDARD1_1
            var channelUrl = userPageHtml.GetElementsBySelector("meta[property=\"og:url\"]")
                .First().GetAttribute("content").Value;
#else
            var channelUrl = userPageHtml.DocumentNode.Descendants("meta")
                .First(e => e.GetAttributeValue("property", "") == "og:url")
                .GetAttributeValue("content", "");
#endif

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
#if NETSTANDARD1_1
            var channelTitle = channelPageHtml.GetElementsBySelector("meta[property=\"og:title\"]")
                .First().GetAttribute("content").Value;

            var channelLogoUrl = channelPageHtml.GetElementsBySelector("meta[property=\"og:image\"]")
                .First().GetAttribute("content").Value;
#else
            var metaNodes = channelPageHtml.DocumentNode.Descendants("meta").ToList();

            string channelTitle = "";
            string channelLogoUrl = "";
            for (int i = 0; i < metaNodes.Count && (channelTitle == "" || channelLogoUrl == ""); i++)
            {
                var prop = metaNodes[i].GetAttributeValue("property", "");
                if (prop == "og:title")
                {
                    channelTitle = metaNodes[i].GetAttributeValue("content", "");
                }
                else if (prop == "og:image")
                {
                    channelLogoUrl = metaNodes[i].GetAttributeValue("content", "");
                }
            }

            // linq is beautiful, but slower (4000 ticks vs 48 ticks)
            /*
            channelTitle = metaNodes.First(e => e.GetAttributeValue("property", "") == "og:title")
                .GetAttributeValue("content", "");

            channelLogoUrl = metaNodes.First(e => e.GetAttributeValue("property", "") == "og:image")
                .GetAttributeValue("content", "");
            */

#endif

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