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
        public VideoEncoding VideoEncoding { get; }

        /// <summary>
        /// Video quality
        /// </summary>
        public VideoQuality VideoQuality { get; }

        /// <summary>
        /// Video resoution
        /// </summary>
        public VideoResolution VideoResolution { get; }

        /// <summary>
        /// Video framerate (fps)
        /// </summary>
        public double VideoFramerate { get; }

        /// <inheritdoc />
        public VideoStreamInfo(int itag, string url, long contentLength, long bitrate, VideoResolution videoResolution, double videoFramerate) 
            : base(itag, url, contentLength)
        {
            Bitrate = bitrate;
            VideoEncoding = GetVideoEncoding(itag);
            VideoQuality = GetVideoQuality(itag);
            VideoResolution = videoResolution;
            VideoFramerate = videoFramerate;
        }
    }
}