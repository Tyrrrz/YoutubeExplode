using System.Collections.Generic;

namespace YoutubeExplode.Common
{
    /// <summary>
    /// Implemented by resources that have thumbnails.
    /// </summary>
    public interface IHasThumbnails
    {
        /// <summary>
        /// Thumbnails.
        /// </summary>
        IReadOnlyList<Thumbnail> Thumbnails { get; }
    }
}