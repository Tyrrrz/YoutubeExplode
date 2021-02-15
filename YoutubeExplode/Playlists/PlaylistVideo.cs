using System;
using YoutubeExplode.Channels;
using YoutubeExplode.Common;
using YoutubeExplode.Videos;

namespace YoutubeExplode.Playlists
{
    /// <summary>
    /// YouTube video metadata from playlists and search results.
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
        /// Duration of the video.
        /// </summary>
        public TimeSpan Duration { get; }

        /// <summary>
        /// View count.
        /// </summary>
        public long ViewCount { get; }

        /// <summary>
        /// Available thumbnails for this video.
        /// </summary>
        public ThumbnailSet Thumbnails { get; }

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
            ThumbnailSet thumbnails)
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
        public override string ToString() => $"Playlist Video ({Title})";
    }
}
