using System;
using Newtonsoft.Json;
using YoutubeExplode.ChanelHandler.ChannelPageModels;

namespace YoutubeExplode.ChanelHandler.Converters
{
    internal class TextUnionConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(TextUnion) || t == typeof(TextUnion?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            switch (reader.TokenType)
            {
                case JsonToken.String:
                case JsonToken.Date:
                    var stringValue = serializer.Deserialize<string>(reader);
                    switch (stringValue)
                    {
                        case " video":
                            return new TextUnion { Enum = TextText.Video };
                        case " videos":
                            return new TextUnion { Enum = TextText.Videos };
                    }
                    long l;
                    if (Int64.TryParse(stringValue, out l))
                    {
                        return new TextUnion { Integer = l };
                    }
                    break;
            }
            throw new Exception("Cannot unmarshal type TextUnion");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            var value = (TextUnion)untypedValue;
            if (value.Enum != null)
            {
                switch (value.Enum)
                {
                    case TextText.Video:
                        serializer.Serialize(writer, " video");
                        return;
                    case TextText.Videos:
                        serializer.Serialize(writer, " videos");
                        return;
                }
            }
            if (value.Integer != null)
            {
                serializer.Serialize(writer, value.Integer.Value.ToString());
                return;
            }
            throw new Exception("Cannot marshal type TextUnion");
        }

        public static readonly TextUnionConverter Singleton = new TextUnionConverter();
    }
}