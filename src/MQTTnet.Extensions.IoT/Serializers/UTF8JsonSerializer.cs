using Google.Protobuf;
using MQTTnet.Extensions.IoT.Binders.WritableProperty;
using System.Text;
using System.Text.Json;

namespace MQTTnet.Extensions.IoT.Serializers;

public class UTF8JsonSerializer : IMessageSerializer
{
    public T FromBytes<T>(byte[] payload, string name = "")
    {
        if (string.IsNullOrEmpty(name))
        {
            return JsonSerializer.Deserialize<T>(Encoding.UTF8.GetString(payload))!;
        }
        JsonDocument jdoc = JsonDocument.Parse(payload);
        return jdoc.RootElement.GetProperty(name).Deserialize<T>()!;
    }
    public byte[] ToBytes<T>(T payload, string name = "")
    {
        if (string.IsNullOrEmpty(name))
        {
            return Encoding.UTF8.GetBytes(JsonSerializer.Serialize(payload));
        }
        else 
        {
            var patch = new Dictionary<string, T> { { name, payload } };
            return Encoding.UTF8.GetBytes(JsonSerializer.Serialize(patch));
        }
    }
}
