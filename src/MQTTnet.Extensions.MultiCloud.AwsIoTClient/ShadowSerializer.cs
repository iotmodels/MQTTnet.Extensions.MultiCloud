using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MQTTnet.Extensions.MultiCloud.AwsIoTClient;

public class ShadowSerializer //: IMessageSerializer
{
    public int Version { get; set; }
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

    public byte[] ToBytes<T>(T payload, string name = "", int? version = null)
    {
        if (version.HasValue)
        {
            Version = (int)version!;
        }
        if (string.IsNullOrEmpty(name))
        {
            return Encoding.UTF8.GetBytes(Json.Stringify(payload!));
        }
        else
        {
            var patch = new
            {
                state = new
                {
                    reported = new Dictionary<string, T>
                    {
                        {name, payload }
                    }
                }
            };
            return Encoding.UTF8.GetBytes(Json.Stringify(patch));
        }
    }

    public bool TryReadFromBytes<T>(byte[] payload, string name, out T result)
    {
        bool found = false;
        string payloadString = Encoding.UTF8.GetString(payload);
        JsonDocument payloadJson = JsonDocument.Parse(payloadString);
        if (payloadJson.RootElement.GetProperty("version").TryGetInt32(out int v))
        {
            Version = v;
        }
        var state = payloadJson.RootElement.GetProperty("state");
        if (state.TryGetProperty(name, out JsonElement propValue))
        {
            found = true;
            result = propValue.Deserialize<T>()!;
        }
        else
        {
            result = default!;
        }
        return found;
    }
}
