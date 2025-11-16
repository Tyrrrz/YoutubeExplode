using System.Collections.Generic;
using System.Linq;

namespace YoutubeExplode.Utils.Extensions;

internal static class CollectionExtensions
{
    extension<T>(IEnumerable<T?> source)
        where T : class
    {
        public IEnumerable<T> WhereNotNull()
        {
            foreach (var i in source)
            {
                if (i is not null)
                    yield return i;
            }
        }
    }

    extension<T>(IEnumerable<T?> source)
        where T : struct
    {
        public IEnumerable<T> WhereNotNull()
        {
            foreach (var i in source)
            {
                if (i is not null)
                    yield return i.Value;
            }
        }
    }

    extension<T>(IEnumerable<T> source)
        where T : struct
    {
        public T? ElementAtOrNull(int index)
        {
            var sourceAsList = source as IReadOnlyList<T> ?? source.ToArray();
            return index < sourceAsList.Count ? sourceAsList[index] : null;
        }

        public T? FirstOrNull()
        {
            foreach (var i in source)
                return i;

            return null;
        }
    }
}
