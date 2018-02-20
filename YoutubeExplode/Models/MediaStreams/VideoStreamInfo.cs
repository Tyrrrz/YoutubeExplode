using JetBrains.Annotations;
using YoutubeExplode.Internal;

namespace YoutubeExplode.Models.MediaStreams
{
    /// <summary>
    /// Metadata associated with a certain <see cref="MediaStream"/> that contains only video.
    /// </summary>
    public class VideoStreamInfo : MediaStreamInfo
    {
        /// <summary>
        /// Video bitrate (bits/s) of the associated stream.
        /// </summary>
        public long Bitrate { get; }

        /// <summary>
        /// Video encoding of the associated stream.
        /// </summary>
        public VideoEncoding VideoEncoding { get; }

        /// <summary>
        /// Video quality of the associated stream.
        /// </summary>
        public VideoQuality VideoQuality { get; }

        /// <summary>
        /// Video resolution of the associated stream.
        /// </summary>
        public VideoResolution Resolution { get; }

        /// <summary>
        /// Video framerate (FPS) of the associated stream.
        /// </summary>
        public int Framerate { get; }

        /// <summary>
        /// Video quality label of the associated stream.
        /// </summary>
        [NotNull]
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

        /// <summary />
        public VideoStreamInfo(int itag, string url, long size, long bitrate, VideoResolution resolution, int framerate,
            string videoQualityLabel)
            : base(itag, url, size)
        {
            Bitrate = bitrate.GuardNotNegative(nameof(bitrate));
            VideoEncoding = GetVideoEncoding(itag);
            Resolution = resolution;
            Framerate = framerate.GuardNotNegative(nameof(framerate));
            VideoQualityLabel = videoQualityLabel.GuardNotNull(nameof(videoQualityLabel));
            VideoQuality = GetVideoQuality(videoQualityLabel);
        }
    }
}