namespace YoutubeExplode.Videos.Streams
{
    /// <summary>
    /// YouTube media stream that only contains audio.
    /// </summary>
    public class AudioOnlyStreamInfo : IAudioStreamInfo
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

        /// <summary>
        /// Initializes an instance of <see cref="AudioOnlyStreamInfo"/>.
        /// </summary>
        public AudioOnlyStreamInfo(int tag,
            string url,
            Container container,
            FileSize size,
            Bitrate bitrate,
            string audioCodec)
        {
            Tag = tag;
            Url = url;
            Container = container;
            Size = size;
            Bitrate = bitrate;
            AudioCodec = audioCodec;
        }

        /// <inheritdoc />
        public override string ToString() => $"Audio-only ({Tag} | {Container})";
    }
}