using YoutubeExplode.Internal;

namespace YoutubeExplode.Models
{
    /// <summary>
    /// Media stream metadata
    /// </summary>
    public class MediaStreamInfo
    {
        /// <summary>
        /// URL for this video stream
        /// </summary>
        public string Url { get; internal set; }

        /// <summary>
        /// Adaptive mode of this stream
        /// </summary>
        public AdaptiveMode AdaptiveMode => ItagHelper.GetAdaptiveMode(Itag);

        /// <summary>
        /// Whether this stream contains audio
        /// </summary>
        public bool HasAudio => AdaptiveMode.IsEither(AdaptiveMode.None, AdaptiveMode.Audio);

        /// <summary>
        /// Whether this stream contains video
        /// </summary>
        public bool HasVideo => AdaptiveMode.IsEither(AdaptiveMode.None, AdaptiveMode.Video);

        /// <summary>
        /// Container type of this stream
        /// </summary>
        public ContainerType Container => ItagHelper.GetContainerType(Itag);

        /// <summary>
        /// Quality of video in this stream
        /// </summary>
        public VideoQuality Quality => ItagHelper.GetVideoQuality(Itag);

        /// <summary>
        /// Whether the contained video is 3D video
        /// </summary>
        public bool IsVideo3D => ItagHelper.GetIsVideo3D(Itag);

        /// <summary>
        /// Whether the stream represents an ongoing feed
        /// </summary>
        public bool IsLiveStream => ItagHelper.GetIsLiveStream(Itag);

        /// <summary>
        /// Resolution of the contained video.
        /// Some streams may not have this property set.
        /// </summary>
        public Resolution Resolution { get; internal set; }

        /// <summary>
        /// Bitrate (bits per second) of this stream.
        /// Some streams may not have this property set.
        /// </summary>
        public long Bitrate { get; internal set; }

        /// <summary>
        /// Frame rate of the contained video.
        /// Some streams may not have this property set.
        /// </summary>
        public double Fps { get; internal set; }

        /// <summary>
        /// Quality label of this stream as seen on Youtube
        /// </summary>
        public string QualityLabel
        {
            get
            {
                var quality = Quality;

                if (quality == VideoQuality.NoVideo)
                    return "No video";

                if (quality == VideoQuality.Low144)
                    return "144p";

                if (quality == VideoQuality.Low240)
                    return "240p";

                if (quality == VideoQuality.Medium360)
                    return "360p";

                if (quality == VideoQuality.Medium480)
                    return "480p";

                if (quality == VideoQuality.High720)
                    return Fps > 30 ? $"720p{Fps:N0}" : "720p";

                if (quality == VideoQuality.High1080)
                    return Fps > 30 ? $"1080p{Fps:N0}" : "1080p";

                if (quality == VideoQuality.High1440)
                    return Fps > 30 ? $"1440p{Fps:N0}" : "1440p";

                if (quality == VideoQuality.High2160)
                    return Fps > 30 ? $"2160p{Fps:N0}" : "2160p";

                if (quality == VideoQuality.High3072)
                    return Fps > 30 ? $"3072p{Fps:N0}" : "3072p";

                return null;
            }
        }

        /// <summary>
        /// File extension of this stream based on type
        /// </summary>
        public string FileExtension => ItagHelper.GetFileExtension(Itag);

        /// <summary>
        /// File size (in bytes) of this stream
        /// </summary>
        public long FileSize { get; internal set; }

        /// <summary>
        /// Internal type id of this stream
        /// </summary>
        internal int Itag { get; set; }

        /// <summary>
        /// Authorization signature
        /// </summary>
        internal string Signature { get; set; }

        /// <summary>
        /// Whether the signature needs to be deciphered before stream can be accessed by URL
        /// </summary>
        internal bool NeedsDeciphering { get; set; }

        internal MediaStreamInfo() { }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Itag: {Itag}";
        }
    }
}