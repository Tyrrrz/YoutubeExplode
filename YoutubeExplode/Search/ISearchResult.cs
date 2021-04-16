using YoutubeExplode.Common;

namespace YoutubeExplode.Search
{
    /// <summary>
    /// <p>
    ///     Search result.
    /// </p>
    /// <p>
    ///     Can be one of the following:
    ///     <list type="bullet">
    ///         <item><see cref="VideoSearchResult"/></item>
    ///         <item><see cref="PlaylistSearchResult"/></item>
    ///         <item><see cref="ChannelSearchResult"/></item>
    ///     </list>
    /// </p>
    /// </summary>
    public interface ISearchResult : IBatchItem
    {
        /// <summary>
        /// Result URL.
        /// </summary>
        string Url { get; }

        /// <summary>
        /// Result title.
        /// </summary>
        string Title { get; }
    }
}