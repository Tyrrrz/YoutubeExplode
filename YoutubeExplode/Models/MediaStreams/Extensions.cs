using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using YoutubeExplode.Internal;

namespace YoutubeExplode.Models.MediaStreams
{
    /// <summary>
    /// Extensions for <see cref="MediaStreams"/>.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Gets file extension based on container type.
        /// </summary>
        public static string GetFileExtension(this Container container)
        {
            switch (container)
            {
                case Container.Mp4:
                    return "mp4";
                case Container.M4A:
                    return "m4a";
                case Container.WebM:
                    return "webm";
                case Container.Tgpp:
                    return "3gpp";
                case Container.Flv:
                    return "flv";
                default:
                    throw new ArgumentOutOfRangeException(nameof(container), "Unexpected container type.");
            }
        }

        /// <summary>
        /// Gets label for given video quality and framerate, as displayed on YouTube.
        /// </summary>
        public static string GetVideoQualityLabel(this VideoQuality videoQuality, int framerate = 30)
        {
            // Video quality
            string qualityPart;
            switch (videoQuality)
            {
                case VideoQuality.Low144:
                    qualityPart = "144p";
                    break;
                case VideoQuality.Low240:
                    qualityPart = "240p";
                    break;
                case VideoQuality.Medium360:
                    qualityPart = "360p";
                    break;
                case VideoQuality.Medium480:
                    qualityPart = "480p";
                    break;
                case VideoQuality.High720:
                    qualityPart = "720p";
                    break;
                case VideoQuality.High1080:
                    qualityPart = "1080p";
                    break;
                case VideoQuality.High1440:
                    qualityPart = "1440p";
                    break;
                case VideoQuality.High2160:
                    qualityPart = "2160p";
                    break;
                case VideoQuality.High3072:
                    qualityPart = "3072p";
                    break;
                case VideoQuality.High4320:
                    qualityPart = "4320p";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(videoQuality),
                        $"Unexpected video quality [{videoQuality}]");
            }

            // Framerate
            var frameratePart = framerate > 30 ? framerate.ToString() : string.Empty;

            return qualityPart + frameratePart;
        }

        /// <summary>
        /// Gets all available <see cref="MediaStreamInfo"/> in a <see cref="MediaStreamInfoSet"/>.
        /// </summary>
        public static IEnumerable<MediaStreamInfo> GetAll(this MediaStreamInfoSet streamInfoSet)
        {
            streamInfoSet.GuardNotNull(nameof(streamInfoSet));

            foreach (var streamInfo in streamInfoSet.Muxed)
                yield return streamInfo;
            foreach (var streamInfo in streamInfoSet.Audio)
                yield return streamInfo;
            foreach (var streamInfo in streamInfoSet.Video)
                yield return streamInfo;
        }

        /// <summary>
        /// Gets all <see cref="VideoQuality"/>s available in a <see cref="MediaStreamInfoSet"/>.
        /// </summary>
        public static IEnumerable<VideoQuality> GetAllVideoQualities(this MediaStreamInfoSet streamInfoSet)
        {
            streamInfoSet.GuardNotNull(nameof(streamInfoSet));

            var qualities = new HashSet<VideoQuality>();

            foreach (var streamInfo in streamInfoSet.Muxed)
                qualities.Add(streamInfo.VideoQuality);
            foreach (var streamInfo in streamInfoSet.Video)
                qualities.Add(streamInfo.VideoQuality);

            return qualities;
        }

        /// <summary>
        /// Gets the <see cref="MuxedStreamInfo"/> with highest <see cref="VideoQuality"/>.
        /// </summary>
        public static MuxedStreamInfo WithHighestVideoQuality(this IEnumerable<MuxedStreamInfo> streamInfos)
        {
            streamInfos.GuardNotNull(nameof(streamInfos));

            return streamInfos.OrderByDescending(s => s.VideoQuality).FirstOrDefault();
        }

        /// <summary>
        /// Gets the <see cref="VideoStreamInfo"/> with highest <see cref="VideoQuality"/>.
        /// </summary>
        public static VideoStreamInfo WithHighestVideoQuality(this IEnumerable<VideoStreamInfo> streamInfos)
        {
            streamInfos.GuardNotNull(nameof(streamInfos));

            return streamInfos.OrderByDescending(s => s.VideoQuality).FirstOrDefault();
        }

        /// <summary>
        /// Gets the <see cref="AudioStreamInfo"/> with highest bitrate.
        /// </summary>
        public static AudioStreamInfo WithHighestBitrate(this IEnumerable<AudioStreamInfo> streamInfos)
        {
            streamInfos.GuardNotNull(nameof(streamInfos));

            return streamInfos.OrderByDescending(s => s.Bitrate).FirstOrDefault();
        }

        /// <summary>
        /// Gets the <see cref="VideoStreamInfo"/> with highest bitrate.
        /// </summary>
        public static VideoStreamInfo WithHighestBitrate(this IEnumerable<VideoStreamInfo> streamInfos)
        {
            streamInfos.GuardNotNull(nameof(streamInfos));

            return streamInfos.OrderByDescending(s => s.Bitrate).FirstOrDefault();
        }
    }
}