﻿using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace YoutubeExplode.Utils
{
    internal class JsonContent : StringContent
    {
        public JsonContent(object? content)
            : base(JsonSerializer.Serialize(content), Encoding.UTF8, "application/json")
        {
        }
    }
}