using System;
using System.Buffers;

namespace YoutubeExplode.Utils
{
    internal readonly struct PooledBuffer<T> : IDisposable
    {
        public T[] Array { get; }

        public PooledBuffer(int minimumLength) =>
            Array = ArrayPool<T>.Shared.Rent(minimumLength);

        public void Dispose() =>
            ArrayPool<T>.Shared.Return(Array);
    }

    internal static class PooledBuffer
    {
        public static PooledBuffer<byte> ForStream() => new(81920);
    }
}