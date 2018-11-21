using System.Collections.Generic;
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
            // Tgpp gets special treatment
            if (container == Container.Tgpp)
                return "3gpp";

            // Convert to lower case string
            return container.ToString().ToLowerInvariant();
        }

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
        /// Gets the stream with highest video quality.
        /// Returns null if sequence is empty.
        /// </summary>
        public static T WithHighestVideoQuality<T>(this IEnumerable<T> streamInfos) where T : IHasVideo
        {
            streamInfos.GuardNotNull(nameof(streamInfos));
            return streamInfos.OrderByDescending(s => s.VideoQuality).FirstOrDefault();
        }

        /// <summary>
        /// Gets the stream with highest bitrate.
        /// Returns null if sequence is empty.
        /// </summary>
        public static T WithHighestBitrate<T>(this IEnumerable<T> streamInfos) where T : MediaStreamInfo
        {
            streamInfos.GuardNotNull(nameof(streamInfos));
            return streamInfos.OrderByDescending(s => s.Bitrate).FirstOrDefault();
        }
    }
}