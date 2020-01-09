using System;
using Newtonsoft.Json;
using YoutubeExplode.ChanelHandler.ChannelPageModels;

namespace YoutubeExplode.ChanelHandler.Converters
{
    internal class TextTextConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(TextText) || t == typeof(TextText?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            switch (value)
            {
                case " video":
                    return TextText.Video;
                case " videos":
                    return TextText.Videos;
            }
            throw new Exception("Cannot unmarshal type TextText");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (TextText)untypedValue;
            switch (value)
            {
                case TextText.Video:
                    serializer.Serialize(writer, " video");
                    return;
                case TextText.Videos:
                    serializer.Serialize(writer, " videos");
                    return;
            }
            throw new Exception("Cannot marshal type TextText");
        }

        public static readonly TextTextConverter Singleton = new TextTextConverter();
    }
}