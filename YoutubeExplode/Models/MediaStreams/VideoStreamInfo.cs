using YoutubeExplode.Internal;

namespace YoutubeExplode.Models.MediaStreams
{
    /// <summary>
    /// Metadata associated with a certain <see cref="MediaStream"/> that contains only video.
    /// </summary>
    public class VideoStreamInfo : MediaStreamInfo, IHasVideo
    {
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
        /// Initializes an instance of <see cref="VideoStreamInfo"/>.
        /// </summary>
        public VideoStreamInfo(int itag, string url, Container container, long size, long bitrate,
            VideoEncoding videoEncoding, string videoQualityLabel, VideoQuality videoQuality,
            VideoResolution resolution, int framerate)
            : base(itag, url, container, size, bitrate)
        {
            VideoEncoding = videoEncoding;
            VideoQualityLabel = videoQualityLabel.GuardNotNull(nameof(videoQualityLabel));
            VideoQuality = videoQuality;
            Resolution = resolution;
            Framerate = framerate.GuardNotNegative(nameof(framerate));
        }

        /// <inheritdoc />
        public override string ToString() => $"{Itag} ({Container}) [video]";
    }
}