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
    public class PlaylistVideo
    {
        /// <summary>
        /// Video ID.
        /// </summary>
        public VideoId Id { get; }

        /// <summary>
        /// Video URL.
        /// </summary>
        public string Url => $"https://www.youtube.com/watch?v={Id}";

        /// <summary>
        /// Video title.
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// Video author.
        /// </summary>
        public string Author { get; }

        /// <summary>
        /// Video channel ID.
        /// </summary>
        public ChannelId ChannelId { get; }

        /// <summary>
        /// Video description.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Video duration.
        /// </summary>
        public TimeSpan Duration { get; }

        /// <summary>
        /// Video view count.
        /// </summary>
        public long ViewCount { get; }

        /// <summary>
        /// Available thumbnails for the video.
        /// </summary>
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
            TimeSpan duration,
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