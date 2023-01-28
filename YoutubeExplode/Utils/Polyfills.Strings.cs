// ReSharper disable CheckNamespace

#if NETSTANDARD2_0 || NET461
internal static class StringPolyfills
{
    public static bool Contains(this string str, char c) =>
        str.IndexOf(c) >= 0;
}
#endif