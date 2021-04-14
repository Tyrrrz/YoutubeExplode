using System.Collections.Generic;

namespace YoutubeExplode.Search
{
    /// <summary>
    /// Batch of results returned by a search query.
    /// </summary>
    public class SearchResultBatch
    {
        /// <summary>
        /// Search results in the batch.
        /// </summary>
        public IReadOnlyList<ISearchResult> Results { get; }

        /// <summary>
        /// Initializes an instance of <see cref="SearchResultBatch"/>.
        /// </summary>
        public SearchResultBatch(IReadOnlyList<ISearchResult> results) =>
            Results = results;
    }
}