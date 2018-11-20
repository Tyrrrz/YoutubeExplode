using System;
using JetBrains.Annotations;

namespace YoutubeExplode.Internal
{
    internal static class Guards
    {
        [ContractAnnotation("o:null => halt")]
        public static T GuardNotNull<T>([NoEnumeration] this T o, string argName = null) where T : class
        {
            return o ?? throw new ArgumentNullException(argName);
        }

        public static TimeSpan GuardNotNegative(this TimeSpan t, string argName = null)
        {
            return t >= TimeSpan.Zero
                ? t
                : throw new ArgumentOutOfRangeException(argName, t, "Cannot be negative.");
        }

        public static int GuardNotNegative(this int i, string argName = null)
        {
            return i >= 0
                ? i
                : throw new ArgumentOutOfRangeException(argName, i, "Cannot be negative.");
        }

        public static long GuardNotNegative(this long i, string argName = null)
        {
            return i >= 0
                ? i
                : throw new ArgumentOutOfRangeException(argName, i, "Cannot be negative.");
        }

        public static int GuardPositive(this int i, string argName = null)
        {
            return i > 0
                ? i
                : throw new ArgumentOutOfRangeException(argName, i, "Cannot be negative or zero.");
        }

        public static long GuardPositive(this long i, string argName = null)
        {
            return i > 0
                ? i
                : throw new ArgumentOutOfRangeException(argName, i, "Cannot be negative or zero.");
        }
    }
}