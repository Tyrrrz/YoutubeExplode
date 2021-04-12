using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using YoutubeExplode.Channels;
using YoutubeExplode.Common;

namespace YoutubeExplode.Videos
{
    /// <summary>
    /// Metadata associated with a YouTube video.
    /// </summary>
    public class Video : IHasThumbnails
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
        /// Video upload date.
        /// </summary>
        public DateTimeOffset UploadDate { get; }

        /// <summary>
        /// Video description.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Video duration.
        /// </summary>
        public TimeSpan Duration { get; }

        /// <summary>
        /// Available thumbnails for the video.
        /// </summary>
        public IReadOnlyList<Thumbnail> Thumbnails { get; }

        /// <summary>
        /// Available search keywords for the video.
        /// </summary>
        public IReadOnlyList<string> Keywords { get; }

        /// <summary>
        /// Engagement statistics for this video.
        /// </summary>
        public Engagement Engagement { get; }

        /// <summary>
        /// Initializes an instance of <see cref="Video"/>.
        /// </summary>
        public Video(
            VideoId id,
            string title,
            string author,
            ChannelId channelId,
            DateTimeOffset uploadDate,
            string description,
            TimeSpan duration,
            IReadOnlyList<Thumbnail> thumbnails,
            IReadOnlyList<string> keywords,
            Engagement engagement)
        {
            Id = id;
            Title = title;
            Author = author;
            ChannelId = channelId;
            UploadDate = uploadDate;
            Description = description;
            Duration = duration;
            Thumbnails = thumbnails;
            Keywords = keywords;
            Engagement = engagement;
        }

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        public override string ToString() => $"Video ({Title})";
    }
}