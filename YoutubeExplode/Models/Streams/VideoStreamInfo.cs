namespace YoutubeExplode.Models.Streams
{
    /// <summary>
    /// Video stream info
    /// </summary>
    public class VideoStreamInfo : MediaStreamInfo
    {
        /// <summary>
        /// Video bitrate (bits/s)
        /// </summary>
        public long Bitrate { get; }

        /// <summary>
        /// Video encoding
        /// </summary>
        public VideoEncoding Encoding { get; }

        /// <summary>
        /// Video quality
        /// </summary>
        public VideoQuality Quality { get; }

        /// <summary>
        /// Video resoution
        /// </summary>
        public VideoResolution Resolution { get; }

        /// <summary>
        /// Video framerate (fps)
        /// </summary>
        public double Framerate { get; }

        /// <inheritdoc />
        public VideoStreamInfo(int itag, string url, long bitrate, VideoResolution resolution, double framerate) 
            : base(itag, url)
        {
            Bitrate = bitrate;
            Encoding = GetVideoEncoding(itag);
            Quality = GetVideoQuality(itag);
            Resolution = resolution;
            Framerate = framerate;
        }
    }
}