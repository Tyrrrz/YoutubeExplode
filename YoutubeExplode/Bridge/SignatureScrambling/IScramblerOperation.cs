namespace YoutubeExplode.Bridge.SignatureScrambling
{
    internal interface IScramblerOperation
    {
        string Unscramble(string input);
    }
}