using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace YoutubeExplode.Internal
{
    internal class AsyncMultiStream : Stream
    {
        private readonly Queue<Func<Task<Stream>>> _queue;
        private Stream _currentStream;

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;

        public AsyncMultiStream(IEnumerable<Func<Task<Stream>>> streamTaskResolvers)
        {
            _queue = new Queue<Func<Task<Stream>>>(streamTaskResolvers);
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count,
            CancellationToken cancellationToken)
        {
            // If queue is empty - return
            if (!_queue.Any())
                return 0;

            // If current stream is not set - resolve it
            if (_currentStream == null)
                _currentStream = await _queue.Dequeue().Invoke().ConfigureAwait(false);

            // Read from stream
            var bytesRead = await _currentStream.ReadAsync(buffer, offset, count, cancellationToken)
                .ConfigureAwait(false);

            // If nothing was read - move to next stream
            if (bytesRead == 0)
            {
                // Dispose and nullify
                _currentStream.Dispose();
                _currentStream = null;

                // Recursively read next stream
                bytesRead += await ReadAsync(buffer, offset + bytesRead, count - bytesRead, cancellationToken)
                    .ConfigureAwait(false);
            }

            return bytesRead;
        }

        public override int Read(byte[] buffer, int offset, int count) =>
            ReadAsync(buffer, offset, count).GetAwaiter().GetResult();

        protected override void Dispose(bool disposing)
        {
            _currentStream?.Dispose();

            base.Dispose(disposing);
        }

        #region Not supported

        public override long Length => throw new NotSupportedException();

        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override void Flush() => throw new NotSupportedException();

        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

        public override void SetLength(long value) => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        #endregion
    }
}