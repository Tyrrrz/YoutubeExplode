using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace YoutubeExplode.Videos.Streams
{
    /// <summary>
    /// Generic YouTube media stream.
    /// </summary>
    public interface IStreamInfo
    {
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
        internal static bool IsThrottled(this IStreamInfo streamInfo) =>
            !Regex.IsMatch(streamInfo.Url, "ratebypass[=/]yes");

        /// <summary>
        /// Gets the stream with the highest bitrate.
        /// Returns null if the sequence is empty.
        /// </summary>
        public static IStreamInfo? WithHighestBitrate(this IEnumerable<IStreamInfo> streamInfos) =>
            streamInfos.OrderByDescending(s => s.Bitrate).FirstOrDefault();
    }
}