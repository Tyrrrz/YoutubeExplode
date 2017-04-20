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
            Id = id;
            Name = name;
            DisplayName = displayName;
            ChannelTitle = channelTitle;
            IsPaid = isPaid;
            ChannelLogoUrl = channelLogoUrl;
            ChannelBannerUrl = channelBannerUrl;
        }
    }
}