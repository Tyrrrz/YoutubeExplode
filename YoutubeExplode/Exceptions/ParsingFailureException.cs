namespace YoutubeExplode.Exceptions
{
    public class ParsingFailureException : YoutubeExplodeException
    {
        public ParsingFailureException(string message)
            : base(message)
        {
        }
    }
}