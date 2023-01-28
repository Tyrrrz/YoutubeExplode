using System.Diagnostics.CodeAnalysis;

namespace YoutubeExplode.Bridge.Cipher;

internal class SpliceCipherOperation : ICipherOperation
{
    private readonly int _index;

    public SpliceCipherOperation(int index) => _index = index;

    public string Decipher(string input) => input[_index..];

    [ExcludeFromCodeCoverage]
    public override string ToString() => $"Splice ({_index})";
}