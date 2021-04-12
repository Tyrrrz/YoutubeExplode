using System;
using YoutubeExplode.Channels;
using YoutubeExplode.Videos;

namespace YoutubeExplode.Common
{
    /// <summary>
    /// Metadata properties shared by videos of different types.
    /// </summary>
    public interface IVideo : IHasThumbnails
    {
        /// <summary>
        /// Video ID.
        /// </summary>
        VideoId Id { get; }

        /// <summary>
        /// Video URL.
        /// </summary>
        string Url { get; }

        /// <summary>
        /// Video title.
        /// </summary>
        string Title { get; }

        /// <summary>
        /// Video author.
        /// </summary>
        string Author { get; }

        /// <summary>
        /// Video channel ID.
        /// </summary>
        ChannelId ChannelId { get; }

        /// <summary>
        /// Video duration.
        /// </summary>
        /// <remarks>
        /// May be null if the video is a currently ongoing live stream.
        /// </remarks>
        TimeSpan? Duration { get; }
    }
}