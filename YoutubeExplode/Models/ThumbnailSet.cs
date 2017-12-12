﻿using YoutubeExplode.Internal;

namespace YoutubeExplode.Models
{
    /// <summary>
    /// Set of thumbnails for a video.
    /// </summary>
    public class ThumbnailSet
    {
        private readonly string _videoId;

        /// <summary>
        /// Low resolution thumbnail URL.
        /// </summary>
        public string LowResUrl => $"https://img.youtube.com/vi/{_videoId}/default.jpg";

        /// <summary>
        /// Medium resolution thumbnail URL.
        /// </summary>
        public string MediumResUrl => $"https://img.youtube.com/vi/{_videoId}/mqdefault.jpg";

        /// <summary>
        /// High resolution thumbnail URL.
        /// </summary>
        public string HighResUrl => $"https://img.youtube.com/vi/{_videoId}/hqdefault.jpg";

        /// <summary>
        /// Standard resolution thumbnail URL.
        /// Not always available.
        /// </summary>
        public string StandardResUrl => $"https://img.youtube.com/vi/{_videoId}/sddefault.jpg";

        /// <summary>
        /// Max resolution thumbnail URL.
        /// Not always available.
        /// </summary>
        public string MaxResUrl => $"https://img.youtube.com/vi/{_videoId}/maxresdefault.jpg";

        /// <summary />
        public ThumbnailSet(string videoId)
        {
            _videoId = videoId.GuardNotNull(nameof(videoId));
        }
    }
}