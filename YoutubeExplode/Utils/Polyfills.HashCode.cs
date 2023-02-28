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

        public static int Combine<T1, T2, T3>(T1 value1, T2 value2, T3 value3)
        {
            var h1 = value1?.GetHashCode() ?? 0;
            var h2 = value2?.GetHashCode() ?? 0;
            var h3 = value3?.GetHashCode() ?? 0;
            var rol3 = ((uint) h1 << 3) | ((uint) h1 >> 29);
            var rol15 = ((uint) h2 << 15) | ((uint) h2 >> 17);
            return ((int) rol3 + h1) ^ ((int) rol15 + h2) ^ h3;
        }
    }
}
#endif