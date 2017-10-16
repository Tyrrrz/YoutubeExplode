using System;
using System.Collections.Generic;
using YoutubeExplode.Internal;

namespace YoutubeExplode.Models
{
    /// <summary>
    /// Playlist video
    /// </summary>
    public class PlaylistVideo
    {
        /// <summary>
        /// ID
        /// </summary>
        public string Id { get; }

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
        /// Statistics
        /// </summary>
        public Statistics Statistics { get; }

        /// <summary />
        public PlaylistVideo(string id, string title, string description, VideoThumbnails thumbnails, TimeSpan duration,
            IReadOnlyList<string> keywords, Statistics statistics)
        {
            Id = id.EnsureNotNull(nameof(id));
            Title = title.EnsureNotNull(nameof(title));
            Description = description.EnsureNotNull(nameof(description));
            Thumbnails = thumbnails.EnsureNotNull(nameof(thumbnails));
            Duration = duration.EnsureNotNegative(nameof(duration));
            Keywords = keywords.EnsureNotNull(nameof(keywords));
            Statistics = statistics.EnsureNotNull(nameof(statistics));
        }
    }
}