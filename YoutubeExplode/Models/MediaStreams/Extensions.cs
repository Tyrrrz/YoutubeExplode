using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
        /// Gets label for given video quality, as displayed on YouTube.
        /// </summary>
        public static string GetVideoQualityLabel(this VideoQuality videoQuality)
        {
            // Convert to string, strip non-digits and add "p"
            return videoQuality.ToString().StripNonDigit() + "p";
        }

        /// <summary>
        /// Gets label for given video quality and framerate, as displayed on YouTube.
        /// </summary>
        public static string GetVideoQualityLabel(this VideoQuality videoQuality, int framerate)
        {
            framerate.GuardNotNegative(nameof(framerate));

            // Framerate appears only if it's above 30
            if (framerate <= 30)
                return videoQuality.GetVideoQualityLabel();

            // YouTube rounds framerate to nearest next ten
            var framerateRounded = (int) Math.Ceiling(framerate / 10.0) * 10;
            return videoQuality.GetVideoQualityLabel() + framerateRounded;
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

        /// <summary>
        /// Gets the expiry date of the stream URL.
        /// Returns null if the expiry date could not be parsed.
        /// </summary>
        public static DateTimeOffset? GetUrlExpiryDate(this MediaStreamInfo streamInfo)
        {
            streamInfo.GuardNotNull(nameof(streamInfo));

            var expiryDateUnix = Regex.Match(streamInfo.Url, @"expire[=/](\d+)").Groups[1].Value.ParseLongOrDefault();
            if (expiryDateUnix == 0L)
                return null;

            return new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero).AddSeconds(expiryDateUnix);
        }
    }
}