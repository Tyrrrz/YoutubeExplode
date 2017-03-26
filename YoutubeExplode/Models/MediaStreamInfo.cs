using YoutubeExplode.Internal;

namespace YoutubeExplode.Models
{
    /// <summary>
    /// Media stream metadata
    /// </summary>
    public class MediaStreamInfo
    {
        #region General

        /// <summary>
        /// Stream URL
        /// </summary>
        public string Url { get; internal set; }

        /// <summary>
        /// Content type
        /// </summary>
        public MediaStreamContentType ContentType => ItagHelper.GetContentType(Itag);

        /// <summary>
        /// Whether this stream contains video
        /// </summary>
        public bool ContainsVideo
        {
            get
            {
                var contentType = ContentType;
                return contentType == MediaStreamContentType.Mixed || contentType == MediaStreamContentType.Video;
            }
        }

        /// <summary>
        /// Whether this stream contains audio
        /// </summary>
        public bool ContainsAudio
        {
            get
            {
                var contentType = ContentType;
                return contentType == MediaStreamContentType.Mixed || contentType == MediaStreamContentType.Audio;
            }
        }

        /// <summary>
        /// Container type
        /// </summary>
        public MediaStreamContainerType ContainerType => ItagHelper.GetContainerType(Itag);

        /// <summary>
        /// Bitrate (bits per second).
        /// Some streams may not have this property set.
        /// </summary>
        public long Bitrate { get; internal set; }

        /// <summary>
        /// File extension, based on container type
        /// </summary>
        public string FileExtension => ItagHelper.GetFileExtension(Itag);

        /// <summary>
        /// File size (in bytes)
        /// </summary>
        public long FileSize { get; internal set; }

        #endregion

        #region Video-related

        /// <summary>
        /// Video quality
        /// </summary>
        public MediaStreamVideoQuality Quality => ItagHelper.GetVideoQuality(Itag);

        /// <summary>
        /// Whether the contained video is a 3D video
        /// </summary>
        public bool IsVideo3D => ItagHelper.GetIsVideo3D(Itag);

        /// <summary>
        /// Resolution of the contained video.
        /// Some streams may not have this property set.
        /// </summary>
        public MediaStreamVideoResolution Resolution { get; internal set; }

        /// <summary>
        /// Frame rate of the contained video.
        /// Some streams may not have this property set.
        /// </summary>
        public double Framerate { get; internal set; }

        /// <summary>
        /// Video quality label as seen on Youtube
        /// </summary>
        public string QualityLabel
        {
            get
            {
                var quality = Quality;

                if (quality == MediaStreamVideoQuality.NoVideo)
                    return "No video";

                if (quality == MediaStreamVideoQuality.Low144)
                    return "144p";

                if (quality == MediaStreamVideoQuality.Low240)
                    return "240p";

                if (quality == MediaStreamVideoQuality.Medium360)
                    return "360p";

                if (quality == MediaStreamVideoQuality.Medium480)
                    return "480p";

                if (quality == MediaStreamVideoQuality.High720)
                    return Framerate > 30 ? $"720p{Framerate:N0}" : "720p";

                if (quality == MediaStreamVideoQuality.High1080)
                    return Framerate > 30 ? $"1080p{Framerate:N0}" : "1080p";

                if (quality == MediaStreamVideoQuality.High1440)
                    return Framerate > 30 ? $"1440p{Framerate:N0}" : "1440p";

                if (quality == MediaStreamVideoQuality.High2160)
                    return Framerate > 30 ? $"2160p{Framerate:N0}" : "2160p";

                if (quality == MediaStreamVideoQuality.High3072)
                    return Framerate > 30 ? $"3072p{Framerate:N0}" : "3072p";

                return null;
            }
        }

        #endregion

        /// <summary>
        /// Internal type ID
        /// </summary>
        internal int Itag { get; set; }

        /// <summary>
        /// Authorization signature
        /// </summary>
        internal string Signature { get; set; }

        /// <summary>
        /// Whether the signature needs to be deciphered
        /// </summary>
        internal bool NeedsDeciphering { get; set; }

        internal MediaStreamInfo()
        {
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{QualityLabel} | {ContentType} | {ContainerType}";
        }
    }
}