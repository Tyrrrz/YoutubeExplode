using System.Collections.Generic;

namespace YoutubeExplode.Utils.Extensions
{
    internal static class CollectionExtensions
    {
        public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> source) where T : class
        {
            foreach (var i in source)
            {
                if (i is not null)
                    yield return i;
            }
        }
    }
}