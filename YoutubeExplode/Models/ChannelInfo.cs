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
        public ChannelInfo(string id, string name, string title, bool isPaid, string logoUrl, string bannerUrl)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Title = title ?? throw new ArgumentNullException(nameof(title));
            IsPaid = isPaid;
            LogoUrl = logoUrl ?? throw new ArgumentNullException(nameof(logoUrl));
            BannerUrl = bannerUrl ?? throw new ArgumentNullException(nameof(bannerUrl));
        }
    }
}