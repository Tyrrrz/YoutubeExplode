using System;

namespace YoutubeExplode.Internal
{
    internal static class Guards
    {
        public static T EnsureNotNull<T>(this T o, string argName = null) where T : class
        {
            if (o == null)
                throw new ArgumentNullException(argName);

            return o;
        }

        public static TimeSpan EnsureNotNegative(this TimeSpan t, string argName = null)
        {
            if (t < TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(argName, t, "Cannot be negative");

            return t;
        }

        public static int EnsureNotNegative(this int i, string argName = null)
        {
            if (i < 0)
                throw new ArgumentOutOfRangeException(argName, i, "Cannot be negative");

            return i;
        }

        public static long EnsureNotNegative(this long i, string argName = null)
        {
            if (i < 0)
                throw new ArgumentOutOfRangeException(argName, i, "Cannot be negative");

            return i;
        }

        public static int EnsurePositive(this int i, string argName = null)
        {
            if (i <= 0)
                throw new ArgumentOutOfRangeException(argName, i, "Cannot be negative or zero");

            return i;
        }
    }
}