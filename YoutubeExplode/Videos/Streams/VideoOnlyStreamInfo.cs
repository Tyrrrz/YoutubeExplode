namespace YoutubeExplode.Videos.Streams
{
    /// <summary>
    /// YouTube media stream that only contains video.
    /// </summary>
    public class VideoOnlyStreamInfo : IVideoStreamInfo
    {
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
        public VideoQuality VideoQuality { get; }

        /// <inheritdoc />
        public VideoResolution VideoResolution { get; }

        /// <summary>
        /// Initializes an instance of <see cref="VideoOnlyStreamInfo"/>.
        /// </summary>
        public VideoOnlyStreamInfo(
            string url,
            Container container,
            FileSize size,
            Bitrate bitrate,
            string videoCodec,
            VideoQuality videoQuality,
            VideoResolution videoResolution)
        {
            Url = url;
            Container = container;
            Size = size;
            Bitrate = bitrate;
            VideoCodec = videoCodec;
            VideoQuality = videoQuality;
            VideoResolution = videoResolution;
        }

        /// <inheritdoc />
        public override string ToString() => $"Video-only ({VideoQuality} | {Container})";
    }
}