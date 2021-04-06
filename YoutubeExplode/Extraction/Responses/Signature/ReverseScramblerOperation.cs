using System.Diagnostics.CodeAnalysis;
using YoutubeExplode.Utils.Extensions;

namespace YoutubeExplode.Extraction.Responses.Signature
{
    internal class ReverseScramblerOperation : IScramblerOperation
    {
        public string Unscramble(string input) => input.Reverse();

        [ExcludeFromCodeCoverage]
        public override string ToString() => "Reverse";
    }
}