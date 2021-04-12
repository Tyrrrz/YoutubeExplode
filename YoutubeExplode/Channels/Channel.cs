using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using YoutubeExplode.Common;

namespace YoutubeExplode.Channels
{
    /// <summary>
    /// Metadata associated with a YouTube channel.
    /// </summary>
    public class Channel : IHasThumbnails
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
        /// Available thumbnails for the channel.
        /// </summary>
        public IReadOnlyList<Thumbnail> Thumbnails { get; }

        /// <summary>
        /// Initializes an instance of <see cref="Channel"/>.
        /// </summary>
        public Channel(ChannelId id, string title, IReadOnlyList<Thumbnail> thumbnails)
        {
            Id = id;
            Title = title;
            Thumbnails = thumbnails;
        }

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        public override string ToString() => $"Channel ({Title})";
    }
}