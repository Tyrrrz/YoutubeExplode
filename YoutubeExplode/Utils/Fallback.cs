using System;
using System.Collections.Generic;
using System.Linq;

namespace YoutubeExplode.Utils
{
    internal static class Fallback
    {
        public static IEnumerable<T> ToEmpty<T>(IEnumerable<T>? maybeSequence) => maybeSequence ?? Enumerable.Empty<T>();

        public static IReadOnlyList<T> ToEmpty<T>(IReadOnlyList<T>? maybeList) => maybeList ?? Array.Empty<T>();
    }
}