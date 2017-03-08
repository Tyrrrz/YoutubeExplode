namespace YoutubeExplode.Internal.CipherOperations
{
    internal interface ICipherOperation
    {
        /// <summary>
        /// Deciphers the given string
        /// </summary>
        string Decipher(string input);
    }
}