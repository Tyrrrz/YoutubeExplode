// ReSharper disable CheckNamespace

#if NETSTANDARD2_0 || NET461
namespace System
{
    internal static class HashCode
    {
        public static int Combine<T>(T value) => value?.GetHashCode() ?? 0;

        public static int Combine<T1, T2>(T1 value1, T2 value2)
        {
            var h1 = value1?.GetHashCode() ?? 0;
            var h2 = value2?.GetHashCode() ?? 0;
            var rol5 = ((uint) h1 << 5) | ((uint) h1 >> 27);
            return ((int) rol5 + h1) ^ h2;
        }
    }
}
#endif