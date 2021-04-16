using System.Diagnostics.CodeAnalysis;
using YoutubeExplode.Channels;

namespace YoutubeExplode.Common
{
    /// <summary>
    /// Author of a YouTube video or playlist.
    /// </summary>
    public class Author
    {
        /// <summary>
        /// Channel ID.
        /// </summary>
        public ChannelId ChannelId { get; }

        /// <summary>
        /// Channel title.
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// Initializes an instance of <see cref="Author"/>.
        /// </summary>
        public Author(ChannelId channelId, string title)
        {
            ChannelId = channelId;
            Title = title;
        }

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        public override string ToString() => Title;
    }
}