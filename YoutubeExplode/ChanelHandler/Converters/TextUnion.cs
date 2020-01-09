using YoutubeExplode.ChanelHandler.ChannelPageModels;

namespace YoutubeExplode.ChanelHandler.Converters
{
    public partial struct TextUnion
    {
        public TextText? Enum;
        public long? Integer;

        public static implicit operator TextUnion(TextText Enum) => new TextUnion { Enum = Enum };
        public static implicit operator TextUnion(long Integer) => new TextUnion { Integer = Integer };
    }
}