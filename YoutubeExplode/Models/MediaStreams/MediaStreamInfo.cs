using System;
using System.Collections.Generic;
using YoutubeExplode.Internal;

namespace YoutubeExplode.Models.MediaStreams
{
    /// <summary>
    /// Base media stream info
    /// </summary>
    public abstract partial class MediaStreamInfo
    {
        /// <summary>
        /// Unique tag that identifies the properties of this stream
        /// </summary>
        public int Itag { get; }

        /// <summary>
        /// Media stream URL
        /// </summary>
        public string Url { get; }

        /// <summary>
        /// Container type
        /// </summary>
        public Container Container { get; }

        /// <summary>
        /// Stream content length (bytes)
        /// </summary>
        public long Size { get; }

        /// <summary />
        protected MediaStreamInfo(int itag, string url, long size)
        {
            Itag = itag;
            Url = url.GuardNotNull(nameof(url));
            Container = GetContainer(itag);
            Size = size.GuardNotNegative(nameof(size));
        }
    }

    public abstract partial class MediaStreamInfo
    {
        private static readonly Dictionary<int, ItagDescriptor> ItagMap = new Dictionary<int, ItagDescriptor>
        {
            // Muxed
            {5, new ItagDescriptor(Container.Flv, AudioEncoding.Mp3, VideoEncoding.H263, VideoQuality.Low144)},
            {6, new ItagDescriptor(Container.Flv, AudioEncoding.Mp3, VideoEncoding.H263, VideoQuality.Low240)},
            {13, new ItagDescriptor(Container.Tgpp, AudioEncoding.Aac, VideoEncoding.Mp4V, VideoQuality.Low144)},
            {17, new ItagDescriptor(Container.Tgpp, AudioEncoding.Aac, VideoEncoding.Mp4V, VideoQuality.Low144)},
            {18, new ItagDescriptor(Container.Mp4, AudioEncoding.Aac, VideoEncoding.H264, VideoQuality.Medium360)},
            {22, new ItagDescriptor(Container.Mp4, AudioEncoding.Aac, VideoEncoding.H264, VideoQuality.High720)},
            {34, new ItagDescriptor(Container.Flv, AudioEncoding.Aac, VideoEncoding.H264, VideoQuality.Medium360)},
            {35, new ItagDescriptor(Container.Flv, AudioEncoding.Aac, VideoEncoding.H264, VideoQuality.Medium480)},
            {36, new ItagDescriptor(Container.Tgpp, AudioEncoding.Aac, VideoEncoding.Mp4V, VideoQuality.Low240)},
            {37, new ItagDescriptor(Container.Mp4, AudioEncoding.Aac, VideoEncoding.H264, VideoQuality.High1080)},
            {38, new ItagDescriptor(Container.Mp4, AudioEncoding.Aac, VideoEncoding.H264, VideoQuality.High3072)},
            {43, new ItagDescriptor(Container.WebM, AudioEncoding.Vorbis, VideoEncoding.Vp8, VideoQuality.Medium360)},
            {44, new ItagDescriptor(Container.WebM, AudioEncoding.Vorbis, VideoEncoding.Vp8, VideoQuality.Medium480)},
            {45, new ItagDescriptor(Container.WebM, AudioEncoding.Vorbis, VideoEncoding.Vp8, VideoQuality.High720)},
            {46, new ItagDescriptor(Container.WebM, AudioEncoding.Vorbis, VideoEncoding.Vp8, VideoQuality.High1080)},
            {59, new ItagDescriptor(Container.Mp4, AudioEncoding.Aac, VideoEncoding.H264, VideoQuality.Medium480)},
            {78, new ItagDescriptor(Container.Mp4, AudioEncoding.Aac, VideoEncoding.H264, VideoQuality.Medium480)},
            {82, new ItagDescriptor(Container.Mp4, AudioEncoding.Aac, VideoEncoding.H264, VideoQuality.Medium360)},
            {83, new ItagDescriptor(Container.Mp4, AudioEncoding.Aac, VideoEncoding.H264, VideoQuality.Medium480)},
            {84, new ItagDescriptor(Container.Mp4, AudioEncoding.Aac, VideoEncoding.H264, VideoQuality.High720)},
            {85, new ItagDescriptor(Container.Mp4, AudioEncoding.Aac, VideoEncoding.H264, VideoQuality.High1080)},
            {91, new ItagDescriptor(Container.Mp4, AudioEncoding.Aac, VideoEncoding.H264, VideoQuality.Low144)},
            {92, new ItagDescriptor(Container.Mp4, AudioEncoding.Aac, VideoEncoding.H264, VideoQuality.Low240)},
            {93, new ItagDescriptor(Container.Mp4, AudioEncoding.Aac, VideoEncoding.H264, VideoQuality.Medium360)},
            {94, new ItagDescriptor(Container.Mp4, AudioEncoding.Aac, VideoEncoding.H264, VideoQuality.Medium480)},
            {95, new ItagDescriptor(Container.Mp4, AudioEncoding.Aac, VideoEncoding.H264, VideoQuality.High720)},
            {96, new ItagDescriptor(Container.Mp4, AudioEncoding.Aac, VideoEncoding.H264, VideoQuality.High1080)},
            {100, new ItagDescriptor(Container.WebM, AudioEncoding.Vorbis, VideoEncoding.Vp8, VideoQuality.Medium360)},
            {101, new ItagDescriptor(Container.WebM, AudioEncoding.Vorbis, VideoEncoding.Vp8, VideoQuality.Medium480)},
            {102, new ItagDescriptor(Container.WebM, AudioEncoding.Vorbis, VideoEncoding.Vp8, VideoQuality.High720)},
            {132, new ItagDescriptor(Container.Mp4, AudioEncoding.Aac, VideoEncoding.H264, VideoQuality.Low240)},
            {151, new ItagDescriptor(Container.Mp4, AudioEncoding.Aac, VideoEncoding.H264, VideoQuality.Low144)},

            // Video-only (mp4)
            {133, new ItagDescriptor(Container.Mp4, null, VideoEncoding.H264, VideoQuality.Low240)},
            {134, new ItagDescriptor(Container.Mp4, null, VideoEncoding.H264, VideoQuality.Medium360)},
            {135, new ItagDescriptor(Container.Mp4, null, VideoEncoding.H264, VideoQuality.Medium480)},
            {136, new ItagDescriptor(Container.Mp4, null, VideoEncoding.H264, VideoQuality.High720)},
            {137, new ItagDescriptor(Container.Mp4, null, VideoEncoding.H264, VideoQuality.High1080)},
            {138, new ItagDescriptor(Container.Mp4, null, VideoEncoding.H264, VideoQuality.High4320)},
            {160, new ItagDescriptor(Container.Mp4, null, VideoEncoding.H264, VideoQuality.Low144)},
            {212, new ItagDescriptor(Container.Mp4, null, VideoEncoding.H264, VideoQuality.Medium480)},
            {213, new ItagDescriptor(Container.Mp4, null, VideoEncoding.H264, VideoQuality.Medium480)},
            {214, new ItagDescriptor(Container.Mp4, null, VideoEncoding.H264, VideoQuality.High720)},
            {215, new ItagDescriptor(Container.Mp4, null, VideoEncoding.H264, VideoQuality.High720)},
            {216, new ItagDescriptor(Container.Mp4, null, VideoEncoding.H264, VideoQuality.High1080)},
            {217, new ItagDescriptor(Container.Mp4, null, VideoEncoding.H264, VideoQuality.High1080)},
            {264, new ItagDescriptor(Container.Mp4, null, VideoEncoding.H264, VideoQuality.High1440)},
            {266, new ItagDescriptor(Container.Mp4, null, VideoEncoding.H264, VideoQuality.High2160)},
            {298, new ItagDescriptor(Container.Mp4, null, VideoEncoding.H264, VideoQuality.High720)},
            {299, new ItagDescriptor(Container.Mp4, null, VideoEncoding.H264, VideoQuality.High1080)},

            // Video-only (webm)
            {167, new ItagDescriptor(Container.WebM, null, VideoEncoding.Vp8, VideoQuality.Medium360)},
            {168, new ItagDescriptor(Container.WebM, null, VideoEncoding.Vp8, VideoQuality.Medium480)},
            {169, new ItagDescriptor(Container.WebM, null, VideoEncoding.Vp8, VideoQuality.High720)},
            {170, new ItagDescriptor(Container.WebM, null, VideoEncoding.Vp8, VideoQuality.High1080)},
            {218, new ItagDescriptor(Container.WebM, null, VideoEncoding.Vp8, VideoQuality.Medium480)},
            {219, new ItagDescriptor(Container.WebM, null, VideoEncoding.Vp8, VideoQuality.Medium480)},
            {242, new ItagDescriptor(Container.WebM, null, VideoEncoding.Vp9, VideoQuality.Low240)},
            {243, new ItagDescriptor(Container.WebM, null, VideoEncoding.Vp9, VideoQuality.Medium360)},
            {244, new ItagDescriptor(Container.WebM, null, VideoEncoding.Vp9, VideoQuality.Medium480)},
            {245, new ItagDescriptor(Container.WebM, null, VideoEncoding.Vp9, VideoQuality.Medium480)},
            {246, new ItagDescriptor(Container.WebM, null, VideoEncoding.Vp9, VideoQuality.Medium480)},
            {247, new ItagDescriptor(Container.WebM, null, VideoEncoding.Vp9, VideoQuality.High720)},
            {248, new ItagDescriptor(Container.WebM, null, VideoEncoding.Vp9, VideoQuality.High1080)},
            {271, new ItagDescriptor(Container.WebM, null, VideoEncoding.Vp9, VideoQuality.High1440)},
            {272, new ItagDescriptor(Container.WebM, null, VideoEncoding.Vp9, VideoQuality.High2160)},
            {278, new ItagDescriptor(Container.WebM, null, VideoEncoding.Vp9, VideoQuality.Low144)},
            {302, new ItagDescriptor(Container.WebM, null, VideoEncoding.Vp9, VideoQuality.High720)},
            {303, new ItagDescriptor(Container.WebM, null, VideoEncoding.Vp9, VideoQuality.High1080)},
            {308, new ItagDescriptor(Container.WebM, null, VideoEncoding.Vp9, VideoQuality.High1440)},
            {313, new ItagDescriptor(Container.WebM, null, VideoEncoding.Vp9, VideoQuality.High2160)},
            {315, new ItagDescriptor(Container.WebM, null, VideoEncoding.Vp9, VideoQuality.High2160)},
            {330, new ItagDescriptor(Container.WebM, null, VideoEncoding.Vp9, VideoQuality.Low144)},
            {331, new ItagDescriptor(Container.WebM, null, VideoEncoding.Vp9, VideoQuality.Low240)},
            {332, new ItagDescriptor(Container.WebM, null, VideoEncoding.Vp9, VideoQuality.Medium360)},
            {333, new ItagDescriptor(Container.WebM, null, VideoEncoding.Vp9, VideoQuality.Medium480)},
            {334, new ItagDescriptor(Container.WebM, null, VideoEncoding.Vp9, VideoQuality.High720)},
            {335, new ItagDescriptor(Container.WebM, null, VideoEncoding.Vp9, VideoQuality.High1080)},
            {336, new ItagDescriptor(Container.WebM, null, VideoEncoding.Vp9, VideoQuality.High1440)},
            {337, new ItagDescriptor(Container.WebM, null, VideoEncoding.Vp9, VideoQuality.High2160)},

            // Audio-only (mp4)
            {139, new ItagDescriptor(Container.M4A, AudioEncoding.Aac, null, null)},
            {140, new ItagDescriptor(Container.M4A, AudioEncoding.Aac, null, null)},
            {141, new ItagDescriptor(Container.M4A, AudioEncoding.Aac, null, null)},
            {256, new ItagDescriptor(Container.M4A, AudioEncoding.Aac, null, null)},
            {258, new ItagDescriptor(Container.M4A, AudioEncoding.Aac, null, null)},
            {325, new ItagDescriptor(Container.M4A, AudioEncoding.Aac, null, null)},
            {328, new ItagDescriptor(Container.M4A, AudioEncoding.Aac, null, null)},

            // Audio-only (webm)
            {171, new ItagDescriptor(Container.WebM, AudioEncoding.Vorbis, null, null)},
            {172, new ItagDescriptor(Container.WebM, AudioEncoding.Vorbis, null, null)},
            {249, new ItagDescriptor(Container.WebM, AudioEncoding.Opus, null, null)},
            {250, new ItagDescriptor(Container.WebM, AudioEncoding.Opus, null, null)},
            {251, new ItagDescriptor(Container.WebM, AudioEncoding.Opus, null, null)}
        };

        /// <summary>
        /// Get container type for the given itag
        /// </summary>
        protected static Container GetContainer(int itag)
        {
            var result = ItagMap.GetOrDefault(itag)?.Container;

            if (!result.HasValue)
                throw new ArgumentOutOfRangeException(nameof(itag), $"Unexpected itag [{itag}]");

            return result.Value;
        }

        /// <summary>
        /// Get encoding for the given itag
        /// </summary>
        protected static AudioEncoding GetAudioEncoding(int itag)
        {
            var result = ItagMap.GetOrDefault(itag)?.AudioEncoding;

            if (!result.HasValue)
                throw new ArgumentOutOfRangeException(nameof(itag), $"Unexpected itag [{itag}]");

            return result.Value;
        }

        /// <summary>
        /// Get encoding for the given itag
        /// </summary>
        protected static VideoEncoding GetVideoEncoding(int itag)
        {
            var result = ItagMap.GetOrDefault(itag)?.VideoEncoding;

            if (!result.HasValue)
                throw new ArgumentOutOfRangeException(nameof(itag), $"Unexpected itag [{itag}]");

            return result.Value;
        }

        /// <summary>
        /// Get video quality for the given itag
        /// </summary>
        protected static VideoQuality GetVideoQuality(int itag)
        {
            var result = ItagMap.GetOrDefault(itag)?.VideoQuality;

            if (!result.HasValue)
                throw new ArgumentOutOfRangeException(nameof(itag), $"Unexpected itag [{itag}]");

            return result.Value;
        }

        /// <summary>
        /// Check if the given itag is known
        /// </summary>
        public static bool IsKnown(int itag)
        {
            return ItagMap.ContainsKey(itag);
        }
    }
}