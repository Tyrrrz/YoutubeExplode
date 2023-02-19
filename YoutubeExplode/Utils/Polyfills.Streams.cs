// ReSharper disable CheckNamespace

#if NETSTANDARD2_0 || NET461
using System.IO;
using System.Threading;
using System.Threading.Tasks;

internal static class StreamPolyfills
{
    public static async Task<int> ReadAsync(this Stream stream, byte[] buffer, CancellationToken cancellationToken) =>
        await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
}
#endif