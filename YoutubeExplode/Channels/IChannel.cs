﻿using System.Collections.Generic;
using YoutubeExplode.Common;

namespace YoutubeExplode.Channels
{
    /// <summary>
    /// Metadata properties shared by channels of different types.
    /// </summary>
    public interface IChannel
    {
        /// <summary>
        /// Channel ID.
        /// </summary>
        ChannelId Id { get; }

        /// <summary>
        /// Channel URL.
        /// </summary>
        string Url { get; }

        /// <summary>
        /// Channel title.
        /// </summary>
        string Title { get; }

        /// <summary>
        /// Channel thumbnails.
        /// </summary>
        IReadOnlyList<Thumbnail> Thumbnails { get; }
    }
}