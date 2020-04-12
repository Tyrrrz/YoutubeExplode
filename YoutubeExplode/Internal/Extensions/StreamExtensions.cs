using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace YoutubeExplode.Internal.Extensions
{
    internal static class StreamExtensions
    {
        public static async Task<int> CopyBufferedToAsync(this Stream source, Stream destination, byte[] buffer,
            CancellationToken cancellationToken = default)
        {
            var bytesCopied = await source.ReadAsync(buffer, cancellationToken);
            await destination.WriteAsync(buffer, 0, bytesCopied, cancellationToken);

            return bytesCopied;
        }

        public static async Task CopyToAsync(this Stream source, Stream destination,
            IProgress<double>? progress = null, CancellationToken cancellationToken = default)
        {
            var buffer = new byte[81920];
            var totalBytesCopied = 0L;
            int bytesCopied;
            do
            {
                // Copy
                bytesCopied = await source.CopyBufferedToAsync(destination, buffer, cancellationToken);

                // Report progress
                totalBytesCopied += bytesCopied;
                progress?.Report(1.0 * totalBytesCopied / source.Length);
            } while (bytesCopied > 0);
        }
    }
}