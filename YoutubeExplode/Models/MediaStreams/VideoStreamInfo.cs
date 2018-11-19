using YoutubeExplode.Internal;

namespace YoutubeExplode.Models.MediaStreams
{
    /// <summary>
    /// Metadata associated with a certain <see cref="MediaStream"/> that contains only video.
    /// </summary>
    public class VideoStreamInfo : MediaStreamInfo, IHasVideo
    {
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
        /// Initializes an instance of <see cref="VideoStreamInfo"/>.
        /// </summary>
        public VideoStreamInfo(string url, long contentLength, long bitrate, string format, string videoEncoding,
            string videoQualityLabel, VideoQuality videoQuality, VideoResolution resolution, int framerate)
            : base(url, contentLength, bitrate, format)
        {
            VideoEncoding = videoEncoding.GuardNotNull(nameof(videoEncoding));
            VideoQualityLabel = videoQualityLabel.GuardNotNull(nameof(videoQualityLabel));
            VideoQuality = videoQuality;
            Resolution = resolution;
            Framerate = framerate;
        }
    }
}