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

        /// <summary>
        /// Regular url of the YouTube watch page for this video
        /// </summary>
        public string RegularUrl {
            get {
                return Id != null ?
                    $"https://www.youtube.com/watch?v={Id}" :
                    null;
            }
        }

        /// <summary>
        /// Short url of the YouTube watch page for this video
        /// </summary>
        public string ShortUrl {
            get {
                return Id != null ?
                    $"https://youtu.be/{Id}" :
                    null;
            }
        }

        /// <summary>
        /// Url of the embedded YouTube watch page for this video
        /// </summary>
        public string EmbedUrl {
            get {
                return Id != null ?
                    $"https://www.youtube.com/embed/{Id}" :
                    null;
            }
        }

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