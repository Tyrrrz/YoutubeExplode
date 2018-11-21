using System;

namespace YoutubeExplode.Models.MediaStreams
{
    /// <summary>
    /// Video encoding.
    /// </summary>
    public enum VideoEncoding
    {
        /// <summary>
        /// MPEG-4 Part 2.
        /// </summary>
        Mp4V,

        /// <summary>
        /// H263.
        /// </summary>
        [Obsolete("Not available anymore.")]
        H263,

        /// <summary>
        /// MPEG-4 Part 10, H264, Advanced Video Coding (AVC).
        /// </summary>
        H264,

        /// <summary>
        /// VP8.
        /// </summary>
        Vp8,

        /// <summary>
        /// VP9.
        /// </summary>
        Vp9,

        /// <summary>
        /// AV1.
        /// </summary>
        Av1
    }
}