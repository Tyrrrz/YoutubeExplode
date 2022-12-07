// ReSharper disable CheckNamespace

#if !NET5_0_OR_GREATER
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

internal static class StreamPolyfills
{
#if NETSTANDARD2_0 || NET461
    public static async Task<int> ReadAsync(this Stream stream, byte[] buffer, CancellationToken cancellationToken) =>
        await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
#endif

    public static async Task<Stream> ReadAsStreamAsync(
        this HttpContent httpContent,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return await httpContent.ReadAsStreamAsync();
    }

    public static async Task<string> ReadAsStringAsync(
        this HttpContent httpContent,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return await httpContent.ReadAsStringAsync();
    }
}
#endif