namespace YoutubeExplode.Videos.Streams
{
    public class AudioOnlyStreamInfo : IAudioStreamInfo
    {
        public int Tag { get; }

        public string Url { get; }

        public Container Container { get; }

        public FileSize Size { get; }

        public Bitrate Bitrate { get; }

        public string AudioCodec { get; }

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
    }
}