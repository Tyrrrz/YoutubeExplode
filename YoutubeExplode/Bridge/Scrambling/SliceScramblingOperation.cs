using System.Diagnostics.CodeAnalysis;

namespace YoutubeExplode.Bridge.Scrambling;

internal class SliceScramblingOperation : IScramblingOperation
{
    private readonly int _index;

    public SliceScramblingOperation(int index) => _index = index;

    public string Unscramble(string input) => input[_index..];

    [ExcludeFromCodeCoverage]
    public override string ToString() => $"Slice ({_index})";
}