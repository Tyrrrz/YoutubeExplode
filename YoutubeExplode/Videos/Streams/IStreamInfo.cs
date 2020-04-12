using System.Collections.Generic;
using System.Linq;

namespace YoutubeExplode.Videos.Streams
{
    /// <summary>
    /// Generic YouTube media stream.
    /// </summary>
    public interface IStreamInfo
    {
        /// <summary>
        /// Stream tag.
        /// Uniquely identifies a stream inside a manifest.
        /// </summary>
        int Tag { get; }

        /// <summary>
        /// Stream URL.
        /// </summary>
        string Url { get; }

        /// <summary>
        /// Stream container.
        /// </summary>
        Container Container { get; }

        /// <summary>
        /// Stream size.
        /// </summary>
        FileSize Size { get; }

        /// <summary>
        /// Stream bitrate.
        /// </summary>
        Bitrate Bitrate { get; }
    }

    /// <summary>
    /// Extensions for <see cref="IStreamInfo"/>.
    /// </summary>
    public static class StreamInfoExtensions
    {
        /// <summary>
        /// Gets the stream with highest bitrate.
        /// Returns null if sequence is empty.
        /// </summary>
        public static IStreamInfo? WithHighestBitrate(this IEnumerable<IStreamInfo> streamInfos) =>
            streamInfos.OrderByDescending(s => s.Bitrate).FirstOrDefault();
    }
}