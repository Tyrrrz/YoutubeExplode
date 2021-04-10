using System.Diagnostics.CodeAnalysis;
using YoutubeExplode.Utils.Extensions;

namespace YoutubeExplode.Bridge.SignatureScrambling
{
    internal class SwapScramblerOperation : IScramblerOperation
    {
        private readonly int _index;

        public SwapScramblerOperation(int index) => _index = index;

        public string Unscramble(string input) => input.SwapChars(0, _index);

        [ExcludeFromCodeCoverage]
        public override string ToString() => $"Swap ({_index})";
    }
}