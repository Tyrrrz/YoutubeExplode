﻿using System;
using YoutubeExplode.Internal;

namespace YoutubeExplode.Models.Streams
{
    /// <summary>
    /// Base media stream info
    /// </summary>
    public abstract partial class MediaStreamInfo
    {
        /// <summary>
        /// Url
        /// </summary>
        public string Url { get; }

        /// <summary>
        /// Container type
        /// </summary>
        public ContainerType ContainerType { get; }

        /// <summary>
        /// File size (bytes)
        /// </summary>
        public long ContentLength { get; internal set; }

        /// <inheritdoc />
        protected MediaStreamInfo(int itag, string url, long contentLength)
        {
            Url = url;
            ContainerType = GetContainerType(itag);
            ContentLength = contentLength;
        }
    }

    public abstract partial class MediaStreamInfo
    {
        /// <summary>
        /// Get container type for the given itag
        /// </summary>
        protected static ContainerType GetContainerType(int itag)
        {
            if (itag.IsEither(18, 22, 82, 83, 84, 85, 160, 133, 134, 135, 136, 298, 137, 299, 264, 266, 138, 212, 213,
                214, 215, 216, 217))
                return ContainerType.Mp4;

            if (itag.IsEither(140, 141))
                return ContainerType.M4A;

            if (itag.IsEither(43, 100, 278, 242, 243, 244, 247, 248, 271, 313, 272, 302, 303, 308, 315, 330, 331, 332,
                333, 334, 335, 336, 337, 171, 249, 250, 251))
                return ContainerType.WebM;

            if (itag.IsEither(13, 17, 36))
                return ContainerType.Tgpp;

            if (itag.IsEither(5, 6, 34, 35))
                return ContainerType.Flv;

            if (itag.IsEither(91, 92, 93, 94, 95, 96, 127, 128))
                return ContainerType.Ts;

            throw new ArgumentException($"Unknown itag [{itag}]", nameof(itag));
        }

        /// <summary>
        /// Get encoding for the given itag
        /// </summary>
        protected static AudioEncoding GetAudioEncoding(int itag)
        {
            if (itag.IsEither(17, 36, 18, 22, 140, 91, 92, 93, 94, 95, 96))
                return AudioEncoding.Aac;

            if (itag.IsEither(43, 171))
                return AudioEncoding.Vorbis;

            if (itag.IsEither(249, 250, 251))
                return AudioEncoding.Opus;

            throw new ArgumentException($"Unknown itag [{itag}]", nameof(itag));
        }

        /// <summary>
        /// Get encoding for the given itag
        /// </summary>
        protected static VideoEncoding GetVideoEncoding(int itag)
        {
            if (itag.IsEither(17, 36))
                return VideoEncoding.Mp4V;

            if (itag.IsEither(18, 22, 160, 133, 134, 135, 136, 298, 137, 299, 264, 266, 138, 91, 92, 93, 94, 95, 96))
                return VideoEncoding.H264;

            if (itag.IsEither(43))
                return VideoEncoding.Vp8;

            if (itag.IsEither(278, 242, 243, 244, 247, 248, 271, 313, 272, 302, 303, 308, 315, 330, 331, 332, 333, 334,
                335, 336, 337))
                return VideoEncoding.Vp9;

            throw new ArgumentException($"Unknown itag [{itag}]", nameof(itag));
        }

        /// <summary>
        /// Get video quality for the given itag
        /// </summary>
        protected static VideoQuality GetVideoQuality(int itag)
        {
            if (itag.IsEither(17, 91, 160, 219, 278, 330))
                return VideoQuality.Low144;

            if (itag.IsEither(5, 36, 83, 92, 132, 133, 242, 331))
                return VideoQuality.Low240;

            if (itag.IsEither(18, 34, 43, 82, 93, 100, 134, 167, 243, 332))
                return VideoQuality.Medium360;

            if (itag.IsEither(35, 44, 83, 101, 94, 135, 168, 218, 244, 245, 246, 212, 213))
                return VideoQuality.Medium480;

            if (itag.IsEither(22, 45, 84, 102, 95, 136, 169, 244, 247, 298, 302, 334, 214, 215))
                return VideoQuality.High720;

            if (itag.IsEither(37, 46, 85, 96, 137, 299, 170, 248, 303, 335, 216, 217))
                return VideoQuality.High1080;

            if (itag.IsEither(264, 271, 308, 336))
                return VideoQuality.High1440;

            if (itag.IsEither(138, 266, 272, 313, 315, 337))
                return VideoQuality.High2160;

            if (itag.IsEither(38))
                return VideoQuality.High3072;

            throw new ArgumentException($"Unknown itag [{itag}]", nameof(itag));
        }
    }
}