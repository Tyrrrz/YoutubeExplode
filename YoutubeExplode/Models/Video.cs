using System;
using System.Collections.Generic;

namespace YoutubeExplode.Models
{
    /// <summary>
    /// Information about a YouTube video.
    /// </summary>
    public class Video
    {
        /// <summary>
        /// ID of this video.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Author of this video.
        /// </summary>
        public string Author { get; }

        /// <summary>
        /// Upload date of this video.
        /// </summary>
        public DateTimeOffset UploadDate { get; }

        /// <summary>
        /// Title of this video.
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// Description of this video.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Thumbnails of this video.
        /// </summary>
        public ThumbnailSet Thumbnails { get; }

        /// <summary>
        /// Duration of this video.
        /// </summary>
        public TimeSpan Duration { get; }

        /// <summary>
        /// Search keywords of this video.
        /// </summary>
        public IReadOnlyList<string> Keywords { get; }

        /// <summary>
        /// Statistics of this video.
        /// </summary>
        public Statistics Statistics { get; }

        /// <summary>
        /// Initializes an instance of <see cref="Video"/>.
        /// </summary>
        public Video(string id, string author, DateTimeOffset uploadDate, string title, string description,
            ThumbnailSet thumbnails, TimeSpan duration, IReadOnlyList<string> keywords, Statistics statistics)
        {
            Id = id;
            Author = author;
            UploadDate = uploadDate;
            Title = title;
            Description = description;
            Thumbnails = thumbnails;
            Duration = duration;
            Keywords = keywords;
            Statistics = statistics;
        }

        /// <inheritdoc />
        public override string ToString() => Title;
    }
}