using JetBrains.Annotations;
using YoutubeExplode.Internal;

namespace YoutubeExplode.Models.MediaStreams
{
    /// <summary>
    /// Metadata associated with a certain <see cref="MediaStream"/> that contains both audio and video.
    /// </summary>
    public class MuxedStreamInfo : MediaStreamInfo
    {
        /// <summary>
        /// Audio encoding of the associated stream.
        /// </summary>
        public AudioEncoding AudioEncoding { get; }

        /// <summary>
        /// Video encoding of the associated stream.
        /// </summary>
        public VideoEncoding VideoEncoding { get; }

        /// <summary>
        /// Video quality of the associated stream.
        /// </summary>
        public VideoQuality VideoQuality { get; }

        /// <summary>
        /// Video quality label of the associated stream.
        /// </summary>
        [NotNull]
        public string VideoQualityLabel { get; }

        /// <summary />
        public MuxedStreamInfo(int itag, string url, long size)
            : base(itag, url, size)
        {
            AudioEncoding = ItagHelper.GetAudioEncoding(itag);
            VideoEncoding = ItagHelper.GetVideoEncoding(itag);
            VideoQuality = ItagHelper.GetVideoQuality(itag);
            VideoQualityLabel = VideoQuality.GetVideoQualityLabel();
        }
    }
}