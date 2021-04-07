namespace YoutubeExplode.Extractors.Signature
{
    internal interface IScramblerOperation
    {
        string Unscramble(string input);
    }
}