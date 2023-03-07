﻿using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MQTTnet.Extensions.MultiCloud.Serializers;

public class UTF8JsonSerializer<T> : IMessageSerializer<T>
{
    private static class Json
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

    public byte[] ToBytes(T payload, string name = "")
    {
        if (string.IsNullOrEmpty(name))
        {
            if (typeof(T) == typeof(string))
            {
                return Encoding.UTF8.GetBytes((payload as string)!);
            }
            else
            {
                return Encoding.UTF8.GetBytes(Json.Stringify(payload!));
            }
        }
        else
        {
            var patch = new Dictionary<string, T> { { name, payload } };
            return Encoding.UTF8.GetBytes(Json.Stringify(patch));
        }
    }

    public bool TryReadFromBytes(byte[] payload, string name, out T result)
    {
        if (payload == null || payload.Length==0)
        {
            result = default!;
            return true;
        }

        bool found = false;
        //if (typeof(T) == typeof(string))
        //{
        //    result = (T) Convert.ChangeType(Encoding.UTF8.GetString(payload), typeof(T));
        //    return true;
        //}


        if (string.IsNullOrEmpty(name))
        {
            found = true;
            if (typeof(T) == typeof(string))
            {
                result = (T) Convert.ChangeType(Encoding.UTF8.GetString(payload), typeof(T));
                //result = Json.FromString<T>(Encoding.UTF8.GetString(payload))!;

            }
            else
            {
                result = Json.FromString<T>(Encoding.UTF8.GetString(payload))!;
            }
        }
        else
        {
            string payloadString = Encoding.UTF8.GetString(payload);
            //if (typeof(T) == typeof(string))
            //{
            //    found = true;
            //    result = (T) Convert.ChangeType(payloadString, typeof(T));
            //}
            //else
            //{
                JsonDocument payloadJson = JsonDocument.Parse(payloadString);
                if (payloadJson.RootElement.ValueKind == JsonValueKind.Object)
                {
                    if (payloadJson.RootElement.TryGetProperty(name, out JsonElement propValue))
                    {
                        found = true;
                        result = propValue.Deserialize<T>()!;
                    }
                    else
                    {
                        result = default!;
                    }
                }
                else
                {
                    result = payloadJson.Deserialize<T>()!;
                    found = true;
                }
            //}
        }
        return found;
    }
}
