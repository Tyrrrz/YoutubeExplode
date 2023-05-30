using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace YoutubeExplode.Utils.Extensions;

internal static class StreamExtensions
{
    public static async ValueTask CopyToAsync(
        this Stream source,
        Stream destination,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default)
    {
        using var buffer = PooledBuffer.ForStream();

        var totalBytesRead = 0L;
        while (true)
        {
            var bytesRead = await source.ReadAsync(buffer.Array, cancellationToken);
            if (bytesRead <= 0)
                break;

            await destination.WriteAsync(buffer.Array, 0, bytesRead, cancellationToken);

            totalBytesRead += bytesRead;
            progress?.Report(1.0 * totalBytesRead / source.Length);
        }
    }
}