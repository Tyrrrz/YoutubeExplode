using System.Diagnostics.CodeAnalysis;
using YoutubeExplode.Utils.Extensions;

namespace YoutubeExplode.Bridge.Scrambling;

internal class ReverseScramblingOperation : IScramblingOperation
{
    public string Unscramble(string input) => input.Reverse();

    [ExcludeFromCodeCoverage]
    public override string ToString() => "Reverse";
}