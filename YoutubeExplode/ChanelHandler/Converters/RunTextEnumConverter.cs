using System;
using Newtonsoft.Json;
using YoutubeExplode.ChanelHandler.ChannelPageModels;

namespace YoutubeExplode.ChanelHandler.Converters
{
    internal class RunTextEnumConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(RunTextEnum) || t == typeof(RunTextEnum?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            if (value == "View full playlist")
            {
                return RunTextEnum.ViewFullPlaylist;
            }
            throw new Exception("Cannot unmarshal type RunTextEnum");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (RunTextEnum)untypedValue;
            if (value == RunTextEnum.ViewFullPlaylist)
            {
                serializer.Serialize(writer, "View full playlist");
                return;
            }
            throw new Exception("Cannot marshal type RunTextEnum");
        }

        public static readonly RunTextEnumConverter Singleton = new RunTextEnumConverter();
    }
}