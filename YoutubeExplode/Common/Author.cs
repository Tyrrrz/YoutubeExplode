using System.Diagnostics.CodeAnalysis;
using YoutubeExplode.Channels;

namespace YoutubeExplode.Common
{
    /// <summary>
    /// Reference to the channel that uploaded a video or created a playlist.
    /// </summary>
    public class Author
    {
        /// <summary>
        /// Author channel ID.
        /// </summary>
        public ChannelId ChannelId { get; }

        /// <summary>
        /// Author channel title.
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
        public override string ToString() => $"Author ({Title})";
    }
}