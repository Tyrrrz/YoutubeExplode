namespace YoutubeExplode.Channels
{
    /// <summary>
    /// YouTube channel metadata.
    /// </summary>
    public class Channel
    {
        /// <summary>
        /// Channel ID.
        /// </summary>
        public ChannelId Id { get; }

        /// <summary>
        /// Channel URL.
        /// </summary>
        public string Url => $"https://www.youtube.com/channel/{Id}";

        /// <summary>
        /// Channel title.
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// URL of the channel's logo image.
        /// </summary>
        public string LogoUrl { get; }

        /// <summary>
        /// Initializes an instance of <see cref="Channel"/>.
        /// </summary>
        public Channel(ChannelId id, string title, string logoUrl)
        {
            Id = id;
            Title = title;
            LogoUrl = logoUrl;
        }

        /// <inheritdoc />
        public override string ToString() => $"Channel ({Title})";
    }
}