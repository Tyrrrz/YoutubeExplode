using System;
using System.Collections.Generic;
using YoutubeExplode.Internal;

namespace YoutubeExplode.Models
{
    /// <summary>
    /// Video
    /// </summary>
    public class Video
    {
        /// <summary>
        /// ID
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Author channel
        /// </summary>
        public string Author { get; }

        /// <summary>
        /// Title
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// Description
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Thumbnails
        /// </summary>
        public VideoThumbnails Thumbnails { get; }

        /// <summary>
        /// Duration
        /// </summary>
        public TimeSpan Duration { get; }

        /// <summary>
        /// Search keywords
        /// </summary>
        public IReadOnlyList<string> Keywords { get; }

        /// <summary>
        /// Status
        /// </summary>
        public VideoStatus Status { get; }

        /// <summary>
        /// Statistics
        /// </summary>
        public Statistics Statistics { get; }

        /// <summary />
        public Video(string id, string author, string title, string description, VideoThumbnails thumbnails,
            TimeSpan duration, IReadOnlyList<string> keywords, VideoStatus status, Statistics statistics)
        {
            Id = id.GuardNotNull(nameof(id));
            Author = author.GuardNotNull(nameof(author));
            Title = title.GuardNotNull(nameof(title));
            Description = description.GuardNotNull(nameof(description));
            Thumbnails = thumbnails.GuardNotNull(nameof(thumbnails));
            Duration = duration.GuardNotNegative(nameof(duration));
            Keywords = keywords.GuardNotNull(nameof(keywords));
            Status = status.GuardNotNull(nameof(status));
            Statistics = statistics.GuardNotNull(nameof(statistics));
        }

        /// <inheritdoc />
        public override string ToString() => Title;
    }
}