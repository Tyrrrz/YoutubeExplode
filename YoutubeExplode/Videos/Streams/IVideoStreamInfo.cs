using System;
using System.Collections.Generic;
using System.Linq;
using YoutubeExplode.Common;

namespace YoutubeExplode.Videos.Streams
{
    /// <summary>
    /// Metadata associated with a media stream that contains video.
    /// </summary>
    public interface IVideoStreamInfo : IStreamInfo
    {
        /// <summary>
        /// Video codec.
        /// </summary>
        string VideoCodec { get; }

        /// <summary>
        /// Video quality.
        /// </summary>
        VideoQuality VideoQuality { get; }

        /// <summary>
        /// Video resolution.
        /// </summary>
        Resolution VideoResolution { get; }
    }

    /// <summary>
    /// Extensions for <see cref="IVideoStreamInfo"/>.
    /// </summary>
    public static class VideoStreamInfoExtensions
    {
        /// <summary>
        /// Gets the video stream with the highest video quality (including framerate).
        /// Returns null if the sequence is empty.
        /// </summary>
        public static IVideoStreamInfo? TryGetWithHighestVideoQuality(this IEnumerable<IVideoStreamInfo> streamInfos) =>
            streamInfos.OrderByDescending(s => s.VideoQuality).FirstOrDefault();

        /// <summary>
        /// Gets the video stream with the highest video quality (including framerate).
        /// </summary>
        public static IVideoStreamInfo GetWithHighestVideoQuality(this IEnumerable<IVideoStreamInfo> streamInfos) =>
            streamInfos.TryGetWithHighestVideoQuality() ??
            throw new InvalidOperationException("Input stream collection is empty.");
    }
}