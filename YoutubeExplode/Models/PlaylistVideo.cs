using System;
using System.Collections.Generic;
using YoutubeExplode.Internal;

namespace YoutubeExplode.Models
{
    /// <summary>
    /// Information about a YouTube playlist video.
    /// </summary>
    public class PlaylistVideo
    {
        /// <summary>
        /// ID of this video.
        /// </summary>
        public string Id { get; }

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
        public VideoThumbnails Thumbnails { get; }

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

        /// <summary />
        public PlaylistVideo(string id, string title, string description, VideoThumbnails thumbnails, TimeSpan duration,
            IReadOnlyList<string> keywords, Statistics statistics)
        {
            Id = id.GuardNotNull(nameof(id));
            Title = title.GuardNotNull(nameof(title));
            Description = description.GuardNotNull(nameof(description));
            Thumbnails = thumbnails.GuardNotNull(nameof(thumbnails));
            Duration = duration.GuardNotNegative(nameof(duration));
            Keywords = keywords.GuardNotNull(nameof(keywords));
            Statistics = statistics.GuardNotNull(nameof(statistics));
        }

        /// <inheritdoc />
        public override string ToString() => Title;
    }
}