namespace YoutubeExplode.Models.MediaStreams
{
    /// <summary>
    /// Mixed (video and audio) stream info
    /// </summary>
    public class MixedStreamInfo : MediaStreamInfo
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

        /// <inheritdoc />
        public MixedStreamInfo(int itag, string url, long contentLength)
            : base(itag, url, contentLength)
        {
            AudioEncoding = GetAudioEncoding(itag);
            VideoEncoding = GetVideoEncoding(itag);
            VideoQuality = GetVideoQuality(itag);
            VideoQualityLabel = GetVideoQualityLabel(VideoQuality, 0);
        }
    }
}
