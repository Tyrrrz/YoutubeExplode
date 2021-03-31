using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace YoutubeExplode.Utils.Extensions
{
    internal static class ExpressionExtensions
    {
        private static readonly Dictionary<Expression, Delegate> CompiledExpressions = new();

        public static T CompileWithCaching<T>(this Expression<T> expression) where T : Delegate
        {
            if (CompiledExpressions.TryGetValue(expression, out var cachedExpression) &&
                cachedExpression is T convertedCachedExpression)
            {
                return convertedCachedExpression;
            }

            var compiledExpression = expression.Compile();

            CompiledExpressions[expression] = compiledExpression;

            return compiledExpression;
        }
    }
}