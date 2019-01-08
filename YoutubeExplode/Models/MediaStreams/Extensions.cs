using System.Collections.Generic;
using System.Linq;
using YoutubeExplode.Internal;
using YoutubeExplode.Internal.Helpers;

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
            => ContainerHelper.ContainerToFileExtension(container);

        /// <summary>
        /// Gets all available media stream infos in a <see cref="MediaStreamInfoSet"/>.
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
        /// Gets all video qualities available in a <see cref="MediaStreamInfoSet"/>.
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
        /// Gets video quality labels of all streams available in a <see cref="MediaStreamInfoSet"/>.
        /// </summary>
        public static IEnumerable<string> GetAllVideoQualityLabels(this MediaStreamInfoSet streamInfoSet)
        {
            streamInfoSet.GuardNotNull(nameof(streamInfoSet));

            var labels = new HashSet<string>();

            foreach (var streamInfo in streamInfoSet.Muxed)
                labels.Add(streamInfo.VideoQualityLabel);
            foreach (var streamInfo in streamInfoSet.Video)
                labels.Add(streamInfo.VideoQualityLabel);

            return labels;
        }

        /// <summary>
        /// Gets the muxed stream with highest video quality.
        /// Returns null if sequence is empty.
        /// </summary>
        public static MuxedStreamInfo WithHighestVideoQuality(this IEnumerable<MuxedStreamInfo> streamInfos)
        {
            streamInfos.GuardNotNull(nameof(streamInfos));
            return streamInfos.OrderByDescending(s => s.VideoQuality).FirstOrDefault();
        }

        /// <summary>
        /// Gets the video stream with highest video quality.
        /// Returns null if sequence is empty.
        /// </summary>
        public static VideoStreamInfo WithHighestVideoQuality(this IEnumerable<VideoStreamInfo> streamInfos)
        {
            streamInfos.GuardNotNull(nameof(streamInfos));
            return streamInfos.OrderByDescending(s => s.VideoQuality).FirstOrDefault();
        }

        /// <summary>
        /// Gets the audio stream with highest bitrate.
        /// Returns null if sequence is empty.
        /// </summary>
        public static AudioStreamInfo WithHighestBitrate(this IEnumerable<AudioStreamInfo> streamInfos)
        {
            streamInfos.GuardNotNull(nameof(streamInfos));
            return streamInfos.OrderByDescending(s => s.Bitrate).FirstOrDefault();
        }

        /// <summary>
        /// Gets the video stream with highest bitrate.
        /// Returns null if sequence is empty.
        /// </summary>
        public static VideoStreamInfo WithHighestBitrate(this IEnumerable<VideoStreamInfo> streamInfos)
        {
            streamInfos.GuardNotNull(nameof(streamInfos));
            return streamInfos.OrderByDescending(s => s.Bitrate).FirstOrDefault();
        }
    }
}