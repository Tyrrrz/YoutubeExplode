using YoutubeExplode.Internal;

namespace YoutubeExplode.Models.MediaStreams
{
    /// <summary>
    /// Metadata associated with a certain <see cref="MediaStream"/> that contains both audio and video.
    /// </summary>
    public class MuxedStreamInfo : MediaStreamInfo, IHasAudio, IHasVideo
    {
        /// <inheritdoc />
        public string AudioEncoding { get; }

        /// <inheritdoc />
        public string VideoEncoding { get; }

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
        public MuxedStreamInfo(string url, long contentLength, long bitrate, string format, string audioEncoding,
            string videoEncoding, string videoQualityLabel, VideoQuality videoQuality, VideoResolution resolution,
            int framerate)
            : base(url, contentLength, bitrate, format)
        {
            AudioEncoding = audioEncoding.GuardNotNull(nameof(audioEncoding));
            VideoEncoding = videoEncoding.GuardNotNull(nameof(videoEncoding));
            VideoQualityLabel = videoQualityLabel.GuardNotNull(nameof(videoQualityLabel));
            VideoQuality = videoQuality;
            Resolution = resolution;
            Framerate = framerate;
        }
    }
}