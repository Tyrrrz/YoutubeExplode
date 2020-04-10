namespace YoutubeExplode.Videos.Streams
{
    public class MuxedStreamInfo : IAudioStreamInfo, IVideoStreamInfo
    {
        public int Tag { get; }

        public string Url { get; }

        public Container Container { get; }

        public FileSize Size { get; }

        public Bitrate Bitrate { get; }

        public string AudioCodec { get; }

        public string VideoCodec { get; }

        public string VideoQualityLabel { get; }

        public VideoQuality VideoQuality { get; }

        public VideoResolution Resolution { get; }

        public Framerate Framerate { get; }

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
    }
}