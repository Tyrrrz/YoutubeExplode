using System;
using System.Buffers;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace YoutubeExplode.Utils.Extensions;

internal static class StreamExtensions
{
    extension(Stream source)
    {
        public async ValueTask CopyToAsync(
            Stream destination,
            IProgress<double>? progress = null,
            CancellationToken cancellationToken = default
        )
        {
            using var buffer = MemoryPool<byte>.Shared.Rent(81920);

            var totalBytesRead = 0L;
            while (true)
            {
                var bytesRead = await source.ReadAsync(buffer.Memory, cancellationToken);
                if (bytesRead <= 0)
                    break;

                await destination.WriteAsync(buffer.Memory[..bytesRead], cancellationToken);

                totalBytesRead += bytesRead;
                progress?.Report(1.0 * totalBytesRead / source.Length);
            }
        }
    }
}
