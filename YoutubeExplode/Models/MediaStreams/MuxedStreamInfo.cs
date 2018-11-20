using YoutubeExplode.Internal;

namespace YoutubeExplode.Models.MediaStreams
{
    /// <summary>
    /// Metadata associated with a certain <see cref="MediaStream"/> that contains both audio and video.
    /// </summary>
    public class MuxedStreamInfo : MediaStreamInfo, IHasAudio, IHasVideo
    {
        /// <inheritdoc />
        public AudioEncoding AudioEncoding { get; }

        /// <inheritdoc />
        public VideoEncoding VideoEncoding { get; }

        /// <inheritdoc />
        public string VideoQualityLabel { get; }

        /// <inheritdoc />
        public VideoQuality VideoQuality { get; }

        /// <inheritdoc />
        public VideoResolution Resolution { get; }

        /// <inheritdoc />
        public int Framerate { get; }

        /// <summary>
        /// Initializes an instance of <see cref="MuxedStreamInfo"/>.
        /// </summary>
        public MuxedStreamInfo(string url, long size, long bitrate, Container container, AudioEncoding audioEncoding,
            VideoEncoding videoEncoding, string videoQualityLabel, VideoQuality videoQuality, VideoResolution resolution,
            int framerate)
            : base(url, size, bitrate, container)
        {
            AudioEncoding = audioEncoding;
            VideoEncoding = videoEncoding;
            VideoQualityLabel = videoQualityLabel.GuardNotNull(nameof(videoQualityLabel));
            VideoQuality = videoQuality;
            Resolution = resolution;
            Framerate = framerate.GuardNotNegative(nameof(framerate));
        }
    }
}