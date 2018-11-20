using YoutubeExplode.Internal;

namespace YoutubeExplode.Models.MediaStreams
{
    /// <summary>
    /// Metadata associated with a certain <see cref="MediaStream"/> that contains both audio and video.
    /// </summary>
    public class MuxedStreamInfo : MediaStreamInfo, IHasAudio, IHasVideo
    {
        /// <inheritdoc />
        public string AudioCodec { get; }

        /// <inheritdoc />
        public string VideoCodec { get; }

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
        public MuxedStreamInfo(string url, long contentLength, long bitrate, string format, string audioCodec,
            string videoCodec, string videoQualityLabel, VideoQuality videoQuality, VideoResolution resolution,
            int framerate)
            : base(url, contentLength, bitrate, format)
        {
            AudioCodec = audioCodec.GuardNotNull(nameof(audioCodec));
            VideoCodec = videoCodec.GuardNotNull(nameof(videoCodec));
            VideoQualityLabel = videoQualityLabel.GuardNotNull(nameof(videoQualityLabel));
            VideoQuality = videoQuality;
            Resolution = resolution;
            Framerate = framerate;
        }
    }
}