namespace YoutubeExplode.Extraction.Responses.Signature
{
    internal interface IScramblerOperation
    {
        string Unscramble(string input);
    }
}