namespace YoutubeExplode.Models
{
    /// <summary>
    /// User metadata
    /// </summary>
    public class UserInfo
    {
        /// <summary>
        /// ID
        /// </summary>
        public string Id { get; internal set; }

        /// <summary>
        /// Channel URL
        /// </summary>
        public string ChannelUrl => $"https://www.youtube.com/channel/{Id}";

        /// <summary>
        /// Actual username
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// Display name
        /// </summary>
        public string DisplayName { get; internal set; }

        /// <summary>
        /// Channel title
        /// </summary>
        public string ChannelTitle { get; internal set; }

        /// <summary>
        /// Whether this user's channel is paid
        /// </summary>
        public bool IsPaid { get; internal set; }

        /// <summary>
        /// Channel logo URL
        /// </summary>
        public string ChannelLogoUrl { get; internal set; }

        /// <summary>
        /// Channel banner URL
        /// </summary>
        public string ChannelBannerUrl { get; internal set; }

        internal UserInfo()
        {
        }

        /// <inhertidoc />
        public override string ToString()
        {
            return $"{DisplayName}";
        }
    }
}