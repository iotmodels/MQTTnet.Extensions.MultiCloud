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

    // TODO convert to TryReadFromBytes
    public T? FromBytes<T>(byte[] payload, string name = "")
    {
        return Json.FromString<T>(Encoding.UTF8.GetString(payload))!;
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

    public bool TryReadFromBytes<T>(byte[] payload, string name, out T result)
    {
        if (payload == null || payload.Length==0)
        {
            result = default!;
            return false;
        }

        bool found = false;
        if (string.IsNullOrEmpty(name))
        {
            found = true;
            result = Json.FromString<T>(Encoding.UTF8.GetString(payload))!;
        }
        else
        {
            string payloadString = Encoding.UTF8.GetString(payload);
            JsonDocument payloadJson = JsonDocument.Parse(payloadString);
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
        return found;
    }
}
