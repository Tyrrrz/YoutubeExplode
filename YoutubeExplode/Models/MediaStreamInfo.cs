using YoutubeExplode.Internal;

namespace YoutubeExplode.Models
{
    /// <summary>
    /// Media stream meta data
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
        public ContainerType Type => ItagHelper.GetContainerType(Itag);

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
        public string QualityLabel => ItagHelper.GetVideoQualityLabel(Itag);

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