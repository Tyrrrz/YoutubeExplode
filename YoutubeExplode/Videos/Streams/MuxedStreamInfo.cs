namespace YoutubeExplode.Videos.Streams
{
    /// <summary>
    /// YouTube media stream that contains both audio and video.
    /// </summary>
    public class MuxedStreamInfo : IAudioStreamInfo, IVideoStreamInfo
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
        public Framerate Framerate { get; }

        /// <summary>
        /// Initializes an instance of <see cref="MuxedStreamInfo"/>.
        /// </summary>
        public MuxedStreamInfo(int tag,
            string url,
            Container container,
            FileSize size,
            Bitrate bitrate,
            string audioCodec,
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
            AudioCodec = audioCodec;
            VideoCodec = videoCodec;
            VideoQualityLabel = videoQualityLabel;
            VideoQuality = videoQuality;
            Resolution = resolution;
            Framerate = framerate;
        }

        /// <inheritdoc />
        public override string ToString() => $"Muxed ({Tag} / {VideoQualityLabel} / {Container})";
    }
}