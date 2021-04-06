using System.Collections.Generic;
using System.Linq;

namespace YoutubeExplode.Common
{
    /// <summary>
    /// Thumbnail image.
    /// </summary>
    public class Thumbnail
    {
        /// <summary>
        /// URL of the thumbnail.
        /// </summary>
        public string Url { get; }

        /// <summary>
        /// Resolution of the thumbnail.
        /// </summary>
        public Resolution Resolution { get; }

        /// <summary>
        /// Initializes an instance of <see cref="Thumbnail"/>.
        /// </summary>
        public Thumbnail(string url, Resolution resolution)
        {
            Url = url;
            Resolution = resolution;
        }

        /// <inheritdoc />
        public override string ToString() => $"Thumbnail ({Resolution})";
    }

    /// <summary>
    /// Extensions for <see cref="Thumbnail"/>.
    /// </summary>
    public static class ThumbnailExtensions
    {
        /// <summary>
        /// Gets the thumbnail with the highest resolution.
        /// Returns null if the sequence is empty.
        /// </summary>
        public static Thumbnail? WithHighestResolution(this IEnumerable<Thumbnail> thumbnails) =>
            thumbnails.OrderByDescending(t => t.Resolution.Area).FirstOrDefault();
    }
}