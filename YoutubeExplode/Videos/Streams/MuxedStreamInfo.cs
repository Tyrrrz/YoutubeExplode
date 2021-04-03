namespace YoutubeExplode.Videos.Streams
{
    /// <summary>
    /// YouTube media stream that contains both audio and video.
    /// </summary>
    public class MuxedStreamInfo : IAudioStreamInfo, IVideoStreamInfo
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
        public string AudioCodec { get; }

        /// <inheritdoc />
        public string VideoCodec { get; }

        /// <inheritdoc />
        public VideoQuality VideoQuality { get; }

        /// <inheritdoc />
        public VideoResolution VideoResolution { get; }

        /// <summary>
        /// Initializes an instance of <see cref="MuxedStreamInfo"/>.
        /// </summary>
        public MuxedStreamInfo(
            string url,
            Container container,
            FileSize size,
            Bitrate bitrate,
            string audioCodec,
            string videoCodec,
            VideoQuality videoQuality,
            VideoResolution resolution)
        {
            Url = url;
            Container = container;
            Size = size;
            Bitrate = bitrate;
            AudioCodec = audioCodec;
            VideoCodec = videoCodec;
            VideoQuality = videoQuality;
            VideoResolution = resolution;
        }

        /// <inheritdoc />
        public override string ToString() => $"Muxed ({VideoQuality} | {Container})";
    }
}