using YoutubeExplode.Internal;

namespace YoutubeExplode.Models
{
    /// <summary>
    /// Channel
    /// </summary>
    public class Channel
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
        /// Name
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

        /// <summary />
        public Channel(string id, string name, string title, bool isPaid, string logoUrl, string bannerUrl)
        {
            Id = id.GuardNotNull(nameof(id));
            Name = name.GuardNotNull(nameof(name));
            Title = title.GuardNotNull(nameof(title));
            IsPaid = isPaid;
            LogoUrl = logoUrl.GuardNotNull(nameof(logoUrl));
            BannerUrl = bannerUrl.GuardNotNull(nameof(bannerUrl));
        }

        /// <inheritdoc />
        public override string ToString() => Title;
    }
}