using System.Collections.Generic;
using System.Linq;

namespace YoutubeExplode.Internal
{
    internal static class Fallback
    {
        public static IEnumerable<T> ToEmpty<T>(IEnumerable<T>? maybeSequence) => maybeSequence ?? Enumerable.Empty<T>();

        public static IReadOnlyList<T> ToEmpty<T>(IReadOnlyList<T>? maybeList) => maybeList ?? new T[0];
    }
}