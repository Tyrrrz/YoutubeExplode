using System;
using AngleSharp.Dom;

namespace YoutubeExplode.Utils.Extensions
{
    internal static class AngleSharpExtensions
    {
        public static IElement QuerySelectorOrThrow(this IParentNode parent, string selector) =>
            parent.QuerySelector(selector) ??
            throw new InvalidOperationException($"Cannot find any element matching '{selector}'.");

        public static string GetAttributeOrThrow(this IElement element, string attribute) =>
            element.GetAttribute(attribute) ??
            throw new InvalidOperationException($"Cannot find attribute '{attribute}'.");
    }
}