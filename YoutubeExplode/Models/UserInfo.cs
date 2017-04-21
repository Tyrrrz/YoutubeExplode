using System;

namespace YoutubeExplode.Models
{
    /// <summary>
    /// User info
    /// </summary>
    public class UserInfo
    {
        /// <summary>
        /// ID
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Channel URL
        /// </summary>
        public string ChannelUrl => $"https://www.youtube.com/channel/{Id}";

        /// <summary>
        /// Actual username
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Display name
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// Channel title
        /// </summary>
        public string ChannelTitle { get; }

        /// <summary>
        /// Whether this user's channel is paid
        /// </summary>
        public bool IsPaid { get; }

        /// <summary>
        /// Channel logo URL
        /// </summary>
        public string ChannelLogoUrl { get; }

        /// <summary>
        /// Channel banner URL
        /// </summary>
        public string ChannelBannerUrl { get; }

        /// <inheritdoc />
        public UserInfo(string id, string name, string displayName, string channelTitle, bool isPaid,
            string channelLogoUrl, string channelBannerUrl)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Name = name ?? throw new ArgumentNullException(nameof(name));
            DisplayName = displayName ?? throw new ArgumentNullException(nameof(displayName));
            ChannelTitle = channelTitle ?? throw new ArgumentNullException(nameof(channelTitle));
            IsPaid = isPaid;
            ChannelLogoUrl = channelLogoUrl ?? throw new ArgumentNullException(nameof(channelLogoUrl));
            ChannelBannerUrl = channelBannerUrl ?? throw new ArgumentNullException(nameof(channelBannerUrl));
        }
    }
}