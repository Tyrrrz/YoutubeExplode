using JetBrains.Annotations;
using YoutubeExplode.Internal;

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
        [NotNull]
        public string Id { get; }

        /// <summary>
        /// Title of this channel.
        /// </summary>
        [NotNull]
        public string Title { get; }

        /// <summary>
        /// Logo image URL of this channel.
        /// </summary>
        [NotNull]
        public string LogoUrl { get; }

        /// <summary>
        /// Initializes an instance of <see cref="Channel"/>.
        /// </summary>
        public Channel(string id, string title, string logoUrl)
        {
            Id = id.GuardNotNull(nameof(id));
            Title = title.GuardNotNull(nameof(title));
            LogoUrl = logoUrl.GuardNotNull(nameof(logoUrl));
        }

        /// <inheritdoc />
        public override string ToString() => Title;
    }
}