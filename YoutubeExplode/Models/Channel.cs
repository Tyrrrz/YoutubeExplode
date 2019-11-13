namespace YoutubeExplode.Models
{
    /// <summary>
    /// Information about a YouTube channel.
    /// </summary>
    public class Channel
    {
        /// <summary>
        /// ID of this channel.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Title of this channel.
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// Logo image URL of this channel.
        /// </summary>
        public string LogoUrl { get; }

        /// <summary>
        /// Initializes an instance of <see cref="Channel"/>.
        /// </summary>
        public Channel(string id, string title, string logoUrl)
        {
            Id = id;
            Title = title;
            LogoUrl = logoUrl;
        }

        /// <inheritdoc />
        public override string ToString() => Title;
    }
}