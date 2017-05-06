using System;

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
        public VideoResolution VideoResolution { get; }

        /// <summary>
        /// Video framerate (fps)
        /// </summary>
        public double VideoFramerate { get; }

        /// <summary>
        /// Video quality label as seen on Youtube
        /// </summary>
        public string VideoQualityLabel { get; }

        /// <inheritdoc />
        public VideoStreamInfo(int itag, string url, long contentLength, long bitrate, VideoResolution videoResolution, double videoFramerate) 
            : base(itag, url, contentLength)
        {
            Bitrate = bitrate >= 0 ? bitrate : throw new ArgumentOutOfRangeException(nameof(bitrate));
            VideoEncoding = GetVideoEncoding(itag);
            VideoQuality = GetVideoQuality(itag);
            VideoResolution = videoResolution;
            VideoFramerate = videoFramerate >= 0 ? videoFramerate : throw new ArgumentOutOfRangeException(nameof(videoFramerate));
            VideoQualityLabel = GetVideoQualityLabel(VideoQuality, videoFramerate);
        }
    }
}