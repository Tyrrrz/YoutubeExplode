using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
        public Author Author { get; }

        /// <inheritdoc />
        public TimeSpan? Duration { get; }

        /// <inheritdoc />
        public IReadOnlyList<Thumbnail> Thumbnails { get; }

        /// <summary>
        /// Initializes an instance of <see cref="PlaylistVideo"/>.
        /// </summary>
        public PlaylistVideo(
            VideoId id,
            string title,
            Author author,
            TimeSpan? duration,
            IReadOnlyList<Thumbnail> thumbnails)
        {
            Id = id;
            Title = title;
            Author = author;
            Duration = duration;
            Thumbnails = thumbnails;
        }

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        public override string ToString() => $"Video ({Title})";
    }
}