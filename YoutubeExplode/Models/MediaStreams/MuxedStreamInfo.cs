﻿namespace YoutubeExplode.Models.MediaStreams
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