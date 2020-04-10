namespace YoutubeExplode.Videos.Streams
{
    public class VideoOnlyStreamInfo : IVideoStreamInfo
    {
        public int Tag { get; }

        public string Url { get; }

        public Container Container { get; }

        public FileSize Size { get; }

        public Bitrate Bitrate { get; }

        public string VideoCodec { get; }

        public string VideoQualityLabel { get; }

        public VideoQuality VideoQuality { get; }

        public VideoResolution Resolution { get; }

        public Framerate Framerate { get; }

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
    }
}