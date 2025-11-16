using System;

namespace YoutubeExplode.Utils.Extensions;

internal static class GenericExtensions
{
    extension<TIn>(TIn input)
    {
        public TOut Pipe<TOut>(Func<TIn, TOut> transform) => transform(input);
    }
}
