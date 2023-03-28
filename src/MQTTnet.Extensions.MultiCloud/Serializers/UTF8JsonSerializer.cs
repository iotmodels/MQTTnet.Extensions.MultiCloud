using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MQTTnet.Extensions.MultiCloud.Serializers;

public class UTF8JsonSerializer : IMessageSerializer
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

    public byte[] ToBytes<T>(T payload, string name = "")
    {
        if (payload is null)  return new byte[0];

        if (string.IsNullOrEmpty(name))
        {
            if (typeof(T) == typeof(string))
            {
                return Encoding.UTF8.GetBytes((payload as string)!);
            }
            else if (typeof(T) == typeof(object))
            {
                return Encoding.UTF8.GetBytes(payload.ToString()!);
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

    public bool TryReadFromBytes<T>(byte[] payload, string name, out T result)
    {
        if (payload == null || payload.Length == 0)
        {
            result = default!;
            return true;
        }

        bool found = false;
        if (string.IsNullOrEmpty(name))
        {
            found = true;
            if (typeof(T) == typeof(string))
            {
                result = (T)Convert.ChangeType(Encoding.UTF8.GetString(payload), typeof(T));
            } 
            else if (typeof(T) == typeof(object))
            {
                result = (T)Convert.ChangeType(Encoding.UTF8.GetString(payload), typeof(T))!;
            }
            else
            {
                result = Json.FromString<T>(Encoding.UTF8.GetString(payload))!;
            }
        }
        else
        {
            string payloadString = Encoding.UTF8.GetString(payload);
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

        }
        return found;
    }
}
