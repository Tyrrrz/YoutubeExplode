using System.Diagnostics.CodeAnalysis;
using YoutubeExplode.Utils.Extensions;

namespace YoutubeExplode.Bridge.Scrambling;

internal class SwapScramblingOperation : IScramblingOperation
{
    private readonly int _index;

    public SwapScramblingOperation(int index) => _index = index;

    public string Unscramble(string input) => input.SwapChars(0, _index);

    [ExcludeFromCodeCoverage]
    public override string ToString() => $"Swap ({_index})";
}