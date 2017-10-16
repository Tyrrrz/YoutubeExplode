using YoutubeExplode.Internal;

namespace YoutubeExplode.Models.MediaStreams
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
        public VideoResolution Resolution { get; }

        /// <summary>
        /// Video framerate (fps)
        /// </summary>
        public int Framerate { get; }

        /// <summary>
        /// Video quality label as seen on Youtube
        /// </summary>
        public string VideoQualityLabel { get; }

        /// <summary />
        public VideoStreamInfo(int itag, string url, long size, long bitrate, VideoResolution resolution, int framerate)
            : base(itag, url, size)
        {
            Bitrate = bitrate.GuardNotNegative(nameof(bitrate));
            VideoEncoding = GetVideoEncoding(itag);
            VideoQuality = GetVideoQuality(itag);
            Resolution = resolution;
            Framerate = framerate.GuardNotNegative(nameof(framerate));
            VideoQualityLabel = VideoQuality.GetVideoQualityLabel(framerate);
        }
    }
}