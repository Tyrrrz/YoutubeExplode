using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace YoutubeExplode.Utils.Extensions
{
    internal static class StreamExtensions
    {
        private static async ValueTask<int> CopyBufferedToAsync(
            this Stream source,
            Stream destination,
            byte[] buffer,
            CancellationToken cancellationToken = default)
        {
            var bytesCopied = await source.ReadAsync(buffer, cancellationToken);
            await destination.WriteAsync(buffer, 0, bytesCopied, cancellationToken);

            return bytesCopied;
        }

        public static async ValueTask CopyToAsync(
            this Stream source,
            Stream destination,
            IProgress<double>? progress = null,
            CancellationToken cancellationToken = default)
        {
            using var buffer = PooledBuffer.ForStream();

            var totalBytesCopied = 0L;
            int bytesCopied;
            do
            {
                bytesCopied = await source.CopyBufferedToAsync(destination, buffer.Array, cancellationToken);
                totalBytesCopied += bytesCopied;
                progress?.Report(1.0 * totalBytesCopied / source.Length);
            } while (bytesCopied > 0);
        }
    }
}