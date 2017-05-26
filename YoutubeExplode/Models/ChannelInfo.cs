using System;

namespace YoutubeExplode.Models
{
    /// <summary>
    /// Channel info
    /// </summary>
    public class ChannelInfo
    {
        /// <summary>
        /// ID
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// URL
        /// </summary>
        public string Url => $"https://www.youtube.com/channel/{Id}";

        /// <summary>
        /// Actual username
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Display name
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// Title
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// Whether this channel is paid
        /// </summary>
        public bool IsPaid { get; }

        /// <summary>
        /// Logo image URL
        /// </summary>
        public string LogoUrl { get; }

        /// <summary>
        /// Banner image URL
        /// </summary>
        public string BannerUrl { get; }

        /// <inheritdoc />
        public ChannelInfo(string id, string name, string displayName, string channelTitle, bool isPaid,
            string channelLogoUrl, string channelBannerUrl)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Name = name ?? throw new ArgumentNullException(nameof(name));
            DisplayName = displayName ?? throw new ArgumentNullException(nameof(displayName));
            Title = channelTitle ?? throw new ArgumentNullException(nameof(channelTitle));
            IsPaid = isPaid;
            LogoUrl = channelLogoUrl ?? throw new ArgumentNullException(nameof(channelLogoUrl));
            BannerUrl = channelBannerUrl ?? throw new ArgumentNullException(nameof(channelBannerUrl));
        }
    }
}