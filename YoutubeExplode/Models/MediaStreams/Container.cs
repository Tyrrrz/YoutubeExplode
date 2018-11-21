using System;

namespace YoutubeExplode.Models.MediaStreams
{
    /// <summary>
    /// Media stream container type.
    /// </summary>
    public enum Container
    {
        /// <summary>
        /// MPEG-4 Part 14 (.mp4).
        /// </summary>
        Mp4,

        /// <summary>
        /// MPEG-4 Part 14 audio-only (.m4a).
        /// </summary>
        [Obsolete("Use Mp4 instead.")]
        M4A = Mp4,

        /// <summary>
        /// Web Media (.webm).
        /// </summary>
        WebM,

        /// <summary>
        /// 3rd Generation Partnership Project (.3gpp).
        /// </summary>
        Tgpp,

        /// <summary>
        /// Flash Video (.flv).
        /// </summary>
        [Obsolete("Not available anymore.")]
        Flv
    }
}