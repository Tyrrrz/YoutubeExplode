using JetBrains.Annotations;
using YoutubeExplode.Internal;

namespace YoutubeExplode.Models.MediaStreams
{
    /// <summary>
    /// Metadata associated with a certain <see cref="MediaStream"/> that contains both audio and video.
    /// </summary>
    public class MuxedStreamInfo : MediaStreamInfo
    {
        /// <summary>
        /// Audio encoding of the associated stream.
        /// </summary>
        public AudioEncoding AudioEncoding { get; }

        /// <summary>
        /// Video encoding of the associated stream.
        /// </summary>
        public VideoEncoding VideoEncoding { get; }

        /// <summary>
        /// Video quality label of the associated stream.
        /// </summary>
        [NotNull]
        public string VideoQualityLabel { get; }

        /// <summary>
        /// Video quality of the associated stream.
        /// </summary>
        public VideoQuality VideoQuality { get; }

        /// <summary>
        /// Video resolution of the associated stream.
        /// </summary>
        public VideoResolution Resolution { get; }

        /// <summary>
        /// Initializes an instance of <see cref="MuxedStreamInfo"/>.
        /// </summary>
        public MuxedStreamInfo(int itag, string url, Container container, long size, AudioEncoding audioEncoding,
            VideoEncoding videoEncoding, string videoQualityLabel, VideoQuality videoQuality,
            VideoResolution resolution)
            : base(itag, url, container, size)
        {
            AudioEncoding = audioEncoding;
            VideoEncoding = videoEncoding;
            VideoQualityLabel = videoQualityLabel.GuardNotNull(nameof(videoQualityLabel));
            VideoQuality = videoQuality;
            Resolution = resolution;
        }

        /// <inheritdoc />
        public override string ToString() => $"{Itag} ({Container}) [muxed]";
    }
}