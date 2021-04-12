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
    public class Video : IVideo
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
        /// Video upload date.
        /// </summary>
        public DateTimeOffset UploadDate { get; }

        /// <summary>
        /// Video description.
        /// </summary>
        public string Description { get; }

        /// <inheritdoc />
        public TimeSpan? Duration { get; }

        /// <inheritdoc />
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
            TimeSpan? duration,
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