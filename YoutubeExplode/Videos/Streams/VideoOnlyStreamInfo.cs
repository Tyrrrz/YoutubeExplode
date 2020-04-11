namespace YoutubeExplode.Videos.Streams
{
    /// <summary>
    /// YouTube media stream that only contains video.
    /// </summary>
    public class VideoOnlyStreamInfo : IVideoStreamInfo
    {
        /// <inheritdoc />
        public int Tag { get; }

        /// <inheritdoc />
        public string Url { get; }

        /// <inheritdoc />
        public Container Container { get; }

        /// <inheritdoc />
        public FileSize Size { get; }

        /// <inheritdoc />
        public Bitrate Bitrate { get; }

        /// <inheritdoc />
        public string VideoCodec { get; }

        /// <inheritdoc />
        public string VideoQualityLabel { get; }

        /// <inheritdoc />
        public VideoQuality VideoQuality { get; }

        /// <inheritdoc />
        public VideoResolution Resolution { get; }

        /// <inheritdoc />
        public Framerate Framerate { get; }

        /// <summary>
        /// Initializes an instance of <see cref="VideoOnlyStreamInfo"/>.
        /// </summary>
        public VideoOnlyStreamInfo(int tag,
            string url,
            Container container,
            FileSize size,
            Bitrate bitrate,
            string videoCodec,
            string videoQualityLabel,
            VideoQuality videoQuality,
            VideoResolution resolution,
            Framerate framerate)
        {
            Tag = tag;
            Url = url;
            Container = container;
            Size = size;
            Bitrate = bitrate;
            VideoCodec = videoCodec;
            VideoQualityLabel = videoQualityLabel;
            VideoQuality = videoQuality;
            Resolution = resolution;
            Framerate = framerate;
        }

        /// <inheritdoc />
        public override string ToString() => $"Video-only ({Tag} / {VideoQualityLabel} / {Container})";
    }
}