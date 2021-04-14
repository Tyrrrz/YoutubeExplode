namespace YoutubeExplode.Search
{
    /// <summary>
    /// <p>
    /// Search result.
    /// </p>
    /// <p>
    /// Can be one of the following:
    /// <list type="bullet">
    ///     <item><see cref="SearchResultVideo"/></item>
    ///     <item><see cref="SearchResultPlaylist"/></item>
    ///     <item><see cref="SearchResultChannel"/></item>
    /// </list>
    /// </p>
    /// </summary>
    public interface ISearchResult
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