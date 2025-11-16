using System;

namespace YoutubeExplode.Converter.Utils.Extensions;

internal static class GenericExtensions
{
    extension<TIn>(TIn input)
    {
        public TOut Pipe<TOut>(Func<TIn, TOut> transform) => transform(input);
    }
}
