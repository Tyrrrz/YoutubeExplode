namespace YoutubeExplode.Models.MediaStreams
{
    /// <summary>
    /// Multiplexed (video and audio) stream info
    /// </summary>
    public class MuxedStreamInfo : MediaStreamInfo
    {
        /// <summary>
        /// Audio encoding
        /// </summary>
        public AudioEncoding AudioEncoding { get; }

        /// <summary>
        /// Video encoding
        /// </summary>
        public VideoEncoding VideoEncoding { get; }

        /// <summary>
        /// Video quality
        /// </summary>
        public VideoQuality VideoQuality { get; }

        /// <summary>
        /// Video quality label as seen on Youtube
        /// </summary>
        public string VideoQualityLabel { get; }

        /// <summary />
        public MuxedStreamInfo(int itag, string url, long size)
            : base(itag, url, size)
        {
            AudioEncoding = GetAudioEncoding(itag);
            VideoEncoding = GetVideoEncoding(itag);
            VideoQuality = GetVideoQuality(itag);
            VideoQualityLabel = VideoQuality.GetVideoQualityLabel();
        }
    }
}