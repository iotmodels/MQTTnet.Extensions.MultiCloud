﻿using Google.Protobuf;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MQTTnet.Extensions.MultiCloud.Serializers;



public class UTF8JsonSerializer : IMessageSerializer
{
    static class Json
    {
        public static string Stringify(object o) => JsonSerializer.Serialize(o,
             new JsonSerializerOptions()
             {
                 Converters =
                {
                    new JsonStringEnumConverter()
                }
             });
        public static T FromString<T>(string s) => JsonSerializer.Deserialize<T>(s,
            new JsonSerializerOptions()
            {
                Converters =
                    {
                        new JsonStringEnumConverter()
                    }
            })!;
    }

    public T FromBytes<T>(byte[] payload, string name = "")
    {
        if (string.IsNullOrEmpty(name))
        {
            return Json.FromString<T>(Encoding.UTF8.GetString(payload))!;
        }
        JsonDocument jdoc = JsonDocument.Parse(payload);
        return jdoc.RootElement.GetProperty(name).Deserialize<T>()!;
    }
    public byte[] ToBytes<T>(T payload, string name = "")
    {
        if (string.IsNullOrEmpty(name))
        {
            return Encoding.UTF8.GetBytes(Json.Stringify(payload!));
        }
        else
        {
            var patch = new Dictionary<string, T> { { name, payload } };
            return Encoding.UTF8.GetBytes(Json.Stringify(patch));
        }
    }
}
