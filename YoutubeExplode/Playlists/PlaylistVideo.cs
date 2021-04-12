using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using YoutubeExplode.Channels;
using YoutubeExplode.Common;
using YoutubeExplode.Videos;

namespace YoutubeExplode.Playlists
{
    /// <summary>
    /// Metadata associated with a YouTube video included in a playlist.
    /// </summary>
    public class PlaylistVideo : IVideo
    {
        /// <inheritdoc />
        public VideoId Id { get; }

        /// <inheritdoc />
        public string Url => $"https://www.youtube.com/watch?v={Id}";

        /// <inheritdoc />
        public string Title { get; }

        /// <inheritdoc />
        public string Author { get; }

        /// <inheritdoc />
        public ChannelId ChannelId { get; }

        /// <summary>
        /// Video description.
        /// </summary>
        public string Description { get; }

        /// <inheritdoc />
        public TimeSpan? Duration { get; }

        /// <summary>
        /// Video view count.
        /// </summary>
        public long ViewCount { get; }

        /// <inheritdoc />
        public IReadOnlyList<Thumbnail> Thumbnails { get; }

        /// <summary>
        /// Initializes an instance of <see cref="Video"/>.
        /// </summary>
        public PlaylistVideo(
            VideoId id,
            string title,
            string author,
            ChannelId channelId,
            string description,
            TimeSpan? duration,
            long viewCount,
            IReadOnlyList<Thumbnail> thumbnails)
        {
            Id = id;
            Title = title;
            Author = author;
            ChannelId = channelId;
            Description = description;
            Duration = duration;
            ViewCount = viewCount;
            Thumbnails = thumbnails;
        }

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        public override string ToString() => $"Playlist Video ({Title})";
    }
}