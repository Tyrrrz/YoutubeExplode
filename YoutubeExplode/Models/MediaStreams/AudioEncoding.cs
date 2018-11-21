using System;

namespace YoutubeExplode.Models.MediaStreams
{
    /// <summary>
    /// Audio encoding.
    /// </summary>
    public enum AudioEncoding
    {
        /// <summary>
        /// MPEG-2 Audio Layer III.
        /// </summary>
        [Obsolete("Not available anymore.")]
        Mp3,

        /// <summary>
        /// MPEG-4 Part 3, Advanced Audio Coding (AAC).
        /// </summary>
        Aac,

        /// <summary>
        /// Vorbis.
        /// </summary>
        Vorbis,

        /// <summary>
        /// Opus.
        /// </summary>
        Opus
    }
}